<script setup lang="ts">
import axios from "axios";
import { ref, watch, onMounted } from "vue";
import { HubConnectionBuilder, HubConnection } from "@microsoft/signalr";
import { nextTick } from 'vue';

type TriState = true | false | null;

interface Message {
  id: number;
  text: string;
  sender: string;
  replyTo: ReplyInfo | null;
}

interface ReplyInfo {
  messageReplyId: number;
  replyState: boolean | null;
}


const userNameInput = ref("");
const userName = ref("");
const currentGroup = ref<string>("");
const newMessage = ref("");
const newGroupName = ref<string>("");
const availableGroups = ref<string[]>([]);
// const groupMessages = ref<{ text: string; sender: string; id: number; replyTo: TriState }[]>(
//   []
// );
const groupMessages = ref<Message[]>([]);


const connection = ref<HubConnection | null>(null);

watch(availableGroups, (x) => {
  console.log(x);
});

const setUserName = async () => {
  if (userNameInput.value.trim() !== "") {
    userName.value = userNameInput.value.trim();
    await initializeSignalRConnection();
    await setUserConnection();
    await loadAvailableGroups();
  }
};

const initializeSignalRConnection = async () => {
  connection.value = new HubConnectionBuilder()
    .withUrl("/hub", {
      accessTokenFactory: () => encodeURIComponent(userName.value),
    })
    .build();

  // connection.value.on(
  //   "onMessage",
  //   (message: Message) => {
  //     if (currentGroup.value) {
  //       groupMessages.value.push({
  //       text: message.text,
  //       sender: message.sender,
  //       id: message.id,
  //       replyTo: message.replyTo});
  //     }
  //     scrollToEnd();
  //   }
  // );

  connection.value.on(
    "onMessage",
    (message: Message) => {
      if (currentGroup.value) {
        groupMessages.value.push(message);
      }
      scrollToEnd();
    }
  );

  connection.value.on("onUserJoin", (userName: string, id: number) => {
    if (currentGroup.value) {
      const message = `${userName} has joined the group.`;
      groupMessages.value.push({
        text: message,
        sender: "System",
        id: id,
        replyTo: null,
      });
      scrollToEnd();
    }
  });

  connection.value.on("onUserRemoved", (userName: string, id: number) => {
    if (currentGroup.value) {
      const message = `${userName} has left the group.`;
      groupMessages.value.push({
        text: message,
        sender: "System",
        id: id,
        replyTo: null,
      });
      scrollToEnd();
    }
  });

  connection.value.on("onMessageRemoved", (messageId) => {
    groupMessages.value = groupMessages.value.filter(
      (message) => message.id !== messageId
    );
  });

  connection.value.on("onGroupDeleted", (groupName: string) => {
    if (currentGroup.value === groupName) {
      currentGroup.value = "";
      groupMessages.value = [];
    }
    availableGroups.value = availableGroups.value.filter(
      (group) => group !== groupName
    );
  });

  connection.value.on(
    "onMessageEdited",
    (messageId: number, newText: string) => {
      const message = groupMessages.value.find((msg) => msg.id === messageId);
      if (message) {
        message.text = newText;
      }
    }
  );

  connection.value.onclose(async () => {
    await initializeSignalRConnection();
  });

  try {
    await connection.value.start();
    console.log("SignalR connection started:", connection.value.connectionId);
  } catch (err) {
    console.error("Error starting SignalR connection:", err);
  }

  if (userName.value) {
    await setUserConnection();
  }
};

const scrollToEnd = () => {
  setTimeout(() => {
    var container = document.querySelector(".messages-container");
    container.scrollTop = container.scrollHeight;
  }, 0);
};

const setUserConnection = async () => {
  if (userName.value && connection.value?.connectionId) {
    try {
      await axios.post("/api/Chat/SetUserName", null, {
        params: {
          userName: userName.value,
          connectionId: connection.value.connectionId,
        },
      });
      await loadAvailableGroups();
    } catch (error) {
      console.error("Error setting user connection:", error);
    }
  }
};

const isGroupCreator = ref(false);

const joinGroup = async (group: string) => {
  if (userName.value) {
    try {
      await axios.post("/api/Chat/JoinGroup", null, {
        params: { groupName: group, userName: userName.value },
      });
      currentGroup.value = group;
      await loadGroupMessages(group);

      const response = await axios.get("/api/Chat/GetGroupCreator", {
        params: { groupName: group },
      });
      isGroupCreator.value = response.data === userName.value;
    } catch (error) {
      console.error("Error joining group:", error);
    }
  }
};

const deleteGroup = async () => {
  if (currentGroup.value && userName.value) {
    try {
      const response = await axios.post("/api/Chat/DeleteGroup", null, {
        params: { groupName: currentGroup.value, userName: userName.value },
      });
      console.log("Response from server:", response.data);
      currentGroup.value = "";
      groupMessages.value = [];
      await loadAvailableGroups();
    } catch (error) {
      console.error("Error deleting group:", error);
    }
  }
};

let repliedMessageId = ref(null); 

const replyMessage = async (message) => {
  if (repliedMessageId.value === message.id) {
    repliedMessageId.value = null;
  } else {
    repliedMessageId.value = message.id;
  }
};

const sendMessage = async () => {
  if (currentGroup.value && userName.value) {
    try {
      const params = {
        groupName: currentGroup.value,
        message: newMessage.value,
        userName: userName.value,
      };
      if (repliedMessageId.value) {
        params["messageId"] = repliedMessageId.value;
      }
      await axios.post("/api/Chat/SendMessage", null, { params });
      newMessage.value = "";
      repliedMessageId.value = null;
      scrollToEnd();
    } catch (error) {
      console.error("Error sending message:", error);
    }
  }
};

const loadGroupMessages = async (group) => {
  try {
    const response = await axios.get("/api/Chat/GetGroupMessages", {
      params: { groupName: group },
    });
    groupMessages.value = response.data;
    scrollToEnd();
  } catch (error) {
    console.error("Error loading group messages:", error);
  }
};

const leaveGroup = async () => {
  if (currentGroup.value && userName.value) {
    try {
      const response = await axios.post("/api/Chat/LeaveGroup", null, {
        params: { groupName: currentGroup.value, userName: userName.value },
      });
      console.log("Response from server:", response.data);
      currentGroup.value = "";
      groupMessages.value = [];
      await loadAvailableGroups(); 
    } catch (error) {
      console.error(
        "Error leaving group:",
        error.response ? error.response.data : error.message
      );
    }
  }
};

const createGroup = async () => {
  if (newGroupName.value.trim() !== "") {
    try {
      await axios.post("/api/Chat/CreateGroup", null, {
        params: { groupName: newGroupName.value, creator: userName.value },
      });
      availableGroups.value.push(newGroupName.value);
      await joinGroup(newGroupName.value);
      newGroupName.value = "";
    } catch (error) {
      console.error("Error creating group:", error);
    }
  }
};

const loadAvailableGroups = async () => {
  currentGroup.value = "";
  if (userName.value) {
    try {
      const response = await axios.get("/api/Chat/GetUserGroups", {
        params: { userName: userName.value },
      });
      availableGroups.value = response.data;
      console.log("Available groups loaded:", availableGroups.value);
    } catch (error) {
      console.error("Error loading user groups:", error);
    }
  }
};

const removeMessageForAll = async (messageId) => {
  try {
    const response = await axios.post("/api/Chat/RemoveMessageForAll", null, {
      params: {
        groupName: currentGroup.value,
        messageId: messageId,
      },
    });
    console.log("Response from server:", response.data);
    //await loadGroupMessages(currentGroup.value);
  } catch (error) {
    console.error("Error :", error);
  }
};

const editedMessage = ref(""); 
const isEditing = ref(false); 
let editedMessageId = null; 

// const showDropdownMenuForMessage = ref(null);

// const showDropdownMenu = (message) => {
//   // Показываем меню только для сообщений текущего пользователя
//   if (message.sender === userName.value) {
//     if (showDropdownMenuForMessage.value === message.id) {
//       // Если меню уже показано для этого сообщения, скрываем его
//       showDropdownMenuForMessage.value = null;
//     } else {
//       // Показываем меню для выбранного сообщения
//       showDropdownMenuForMessage.value = message.id;
//     }
//   }
// };

const showDropdownMenuForMessage = ref<number | null>(null);

const showDropdownMenu = (message) => {
  if(message.sender == 'System'){
    showDropdownMenuForMessage.value = null;
  }
  else if (showDropdownMenuForMessage.value === message.id) {
    showDropdownMenuForMessage.value = null;
  } else {
    showDropdownMenuForMessage.value = message.id;
  }
};

const editMessage = async (message) => {
  isEditing.value = true; 
  editedMessage.value = message.text;
  editedMessageId = message.id; 
};

const cancelEdit = () => {
  editedMessage.value = "";
  isEditing.value = false; 
  showDropdownMenuForMessage.value = null;
};

const saveEdit = async () => {
  if (editedMessageId !== null) {
    try {
      await axios.post("/api/Chat/EditMessage", null, {
        params: {
          groupName: currentGroup.value,
          messageId: editedMessageId,
          newText: editedMessage.value,
        },
      });
      editedMessage.value = "";
      isEditing.value = false;
      showDropdownMenuForMessage.value = null;
    } catch (error) {
      console.error("Error editing message:", error);
    }
  }
};

const scrollToMessage = (messageId) => {
  nextTick(() => {
    const messageElement = document.getElementById(`message-${messageId}`);
    if (messageElement) {
      messageElement.scrollIntoView({ behavior: "smooth", block: "center" });
    }
  });
};

const scrollToOriginalMessage = (message) => {
  console.log("scrollToOriginalMessage:");
  if (message.replyTo !== null) {
    console.log("Original message ID:", `message-${message.replyTo.messageReplyId}`);
    const originalMessage = groupMessages.value.find(
      (msg) => msg.id === message.replyTo.messageReplyId
    );
    console.log("Original message:", originalMessage);
    if (originalMessage) {
      nextTick(() => {
        const originalMessageElement = document.querySelector(`[data-id='message-${originalMessage.id}']`);
        console.log("Original message element:", originalMessageElement);
        if (originalMessageElement) {
            originalMessageElement.scrollIntoView({ behavior: "smooth", block: "center" });
            originalMessageElement.classList.add('highlight'); 
            setTimeout(() => {
              originalMessageElement.classList.remove('highlight'); 
            }, 1000);
          }
      });
    }
  }
};





onMounted(async () => {
  if (userName.value) {
    await initializeSignalRConnection();
    await setUserConnection();
    await loadAvailableGroups();
  }
});
</script>

<template>
  <div>
    <h1 class="header">Chat Application</h1>
    <div v-if="!userName">
      <input
        v-focus
        v-model="userNameInput"
        placeholder="Enter your username"
      />
      <button @click="setUserName">Set Username</button>
    </div>
    <div v-else>
      <div v-if="!currentGroup">
        <div>
          <h2>Available Groups</h2>
          <ul class="group-list">
            <li v-for="group in availableGroups" :key="group">
              <button @click="joinGroup(group)">{{ group }}</button>
            </li>
          </ul>
        </div>
        <div>
          <h2>Create a New Group</h2>
          <input
            v-focus
            v-model="newGroupName"
            placeholder="Enter group name"
          />
          <button @click="createGroup">Create Group</button>
          <button @click="joinGroup(newGroupName)" class="join-button">
            Join Group
          </button>
        </div>
      </div>
      <div v-else>
        <div class="chat-container">
          <h2 class="chat-title">{{ currentGroup }}</h2>
          <div class="messages-container">
            <div
              v-for="message in groupMessages"
              :key="message.id"
              :data-id="'message-' + message.id"
              :class="[
                'message',
                {
                  'user-message': message.sender === userName,
                  'other-message': message.sender !== userName,
                  'system-message': message.sender === 'System',
                  'choice-reply-message': repliedMessageId == message.id && repliedMessageId != null,
                  'other-reply-to-message': (message.replyTo && message.replyTo.replyState === false && message.sender == userName) || (message.replyTo && message.replyTo.replyState === true && message.sender != userName),
                  'user-reply-to-message': (message.replyTo && message.replyTo.replyState === false && message.sender != userName) || (message.replyTo && message.replyTo.replyState === true && message.sender == userName),
                },
              ]"

              @contextmenu.prevent.right="showDropdownMenu(message)"
              @dblclick="scrollToOriginalMessage(message)"
            >
              <span v-if="message.sender !== userName" class="sender-name"
                >{{ message.sender }}:</span
              >

              {{ message.text }}

              <div
                v-if="showDropdownMenuForMessage === message.id &&  message.replyTo == null"
                class="dropdown"
              >
                <button
                  v-if="message.sender === userName"
                  @click="removeMessageForAll(message.id)"
                >
                  Удалить
                </button>
                <button
                  v-if="message.sender === userName"
                  @click="editMessage(message)"
                >
                  Редактировать
                </button>
                <button @click="replyMessage(message)">Ответить</button>
              </div>
            </div>
          </div>
          <div class="input-container" v-if="!isEditing">
            <input
              v-focus
              v-model="newMessage"
              placeholder="Type a message..."
              @keyup.enter="sendMessage"
            />
            <button type="submit" @click="sendMessage">Send</button>
          </div>
          <div class="buttons-container">
            <button v-if="isGroupCreator" @click="deleteGroup">
              Delete Group
            </button>
            <button @click="loadAvailableGroups">Back to Menu</button>
            <button @click="leaveGroup">Leave Group</button>
          </div>
        </div>
        <div class="input-container" v-if="isEditing">
          <input
            v-focus
            v-model="editedMessage"
            @keydown.enter="saveEdit"
            @keydown.escape="cancelEdit"
            placeholder="Edit your message..."
          />
          <button @click="cancelEdit">Cancel</button>
          <button @click="saveEdit">Save</button>
        </div>
      </div>
    </div>
  </div>
</template>

<style>
.header {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  background-color: #ffffff;
  padding: 10px 0;
  text-align: center;
}

.group-list {
  list-style-type: none;
}

.group-list li {
  margin-bottom: 5px;
}

.join-button {
  margin-left: 10px;
}

.chat-title {
  position: fixed;
  top: 100px;
  left: 0;
  width: 100%;
  text-align: center;
}

.chat-container {
  margin-top: 50px;
}

.messages-container {
  height: 640px;
  width: 600px;
  overflow-y: auto;
}

.message {
  padding: 10px;
  margin: 5px 0;
  border-radius: 10px;
}

.user-message {
  background-color: #f0f0f0;
  color: #007bff;
  text-align: right;
}

.other-message {
  background-color: #ffffff;
  color: #000000;
  text-align: left;
}

.user-reply-to-message{
  text-align: right;
  background-color: #869898cc;
  margin-right: 20px; 
}

.other-reply-to-message{
  text-align: left;
  background-color: #869898cc;
  margin-left: 20px; 
}

.system-message {
  text-align: center;
  color: #24ab65;
}

.choice-reply-message{
  color: rgb(255, 0, 0);
}

.input-container {
  position: fixed;
  bottom: 100px;
  left: 0;
  width: 100%;
  margin-top: 20px;
  text-align: center;
}

.buttons-container {
  position: absolute;
  top: 120px;
  right: 10px;
}

.highlight {
  background-color: #ffc107; 
}
</style>


