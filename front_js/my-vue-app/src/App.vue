<script setup lang="ts">
import axios from "axios";
import { ref, watch, onMounted } from "vue";
import { HubConnectionBuilder, HubConnection } from "@microsoft/signalr";

const userNameInput = ref("");
const userName = ref("");
const currentGroup = ref<string>("");
const newMessage = ref("");
const newGroupName = ref<string>("");
const availableGroups = ref<string[]>([]);
const groupMessages = ref<{ text: string; sender: string }[]>([]);

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
      accessTokenFactory: () => encodeURIComponent(userName.value), // Encode the username
    })
    .build();

  connection.value.on(
    "onMessage",
    (message: string, sender: string, id: int) => {
      if (currentGroup.value) {
        groupMessages.value.push({ text: message, sender, id });
      }
      scrollToEnd();
    }
  );

  connection.value.on("onUserJoin", (userName: string) => {
    if (currentGroup.value) {
      const message = `${userName} has joined the group.`;
      groupMessages.value.push({ text: message, sender: "System" });
      scrollToEnd();
    }
  });

  connection.value.on("onUserRemoved", (userName: string) => {
    if (currentGroup.value) {
      const message = `${userName} has left the group.`;
      groupMessages.value.push({ text: message, sender: "System" });
      scrollToEnd();
    }
  });

  connection.value.on("onMessageRemoved", (messageId) => {
    groupMessages.value = groupMessages.value.filter(
      (message) => message.id !== messageId
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
    await initializeSignalRConnection(); // Reinitialize connection on disconnect
  });

  try {
    await connection.value.start();
    console.log("SignalR connection started:", connection.value.connectionId);
  } catch (err) {
    console.error("Error starting SignalR connection:", err);
  }

  // Update connection ID if user already exists
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
      await loadAvailableGroups(); // Load groups after setting the user connection
    } catch (error) {
      console.error("Error setting user connection:", error);
    }
  }
};

const joinGroup = async (group: string) => {
  if (userName.value) {
    try {
      await axios.post("/api/Chat/JoinGroup", null, {
        params: { groupName: group, userName: userName.value },
      });
      currentGroup.value = group;
      await loadGroupMessages(group);
    } catch (error) {
      console.error("Error joining group:", error);
    }
  }
};

const sendMessage = async () => {
  if (currentGroup.value && userName.value) {
    try {
      await axios.post("/api/Chat/SendMessage", null, {
        params: {
          groupName: currentGroup.value,
          message: newMessage.value,
          userName: userName.value,
        },
      });
      newMessage.value = "";
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
      await loadAvailableGroups(); // Refresh available groups after leaving
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
        params: { groupName: newGroupName.value },
      });
      // After creating a new group, add it to availableGroups
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

const editedMessage = ref(""); // Store the edited message
const isEditing = ref(false); // Flag to track editing mode
let editedMessageId = null; // S
const showDropdownMenuForMessage = ref(null);

const showDropdownMenu = (message) => {
  // Показываем меню только для сообщений текущего пользователя
  if (message.sender === userName.value) {
    if (showDropdownMenuForMessage.value === message.id) {
      // Если меню уже показано для этого сообщения, скрываем его
      showDropdownMenuForMessage.value = null;
    } else {
      // Показываем меню для выбранного сообщения
      showDropdownMenuForMessage.value = message.id;
    }
  }
};

const editMessage = async (message) => {
  isEditing.value = true; // Enter editing mode
  editedMessage.value = message.text; // Populate the input with the message text
  editedMessageId = message.id; // Store the ID of the message being edited
};

const cancelEdit = () => {
  editedMessage.value = ""; // Clear the edited message
  isEditing.value = false; // Exit editing mode
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
              class="message"
              :class="{
                'user-message': message.sender === userName,
                'other-message': message.sender !== userName,
                'system-message': message.sender === 'System' 
              }"
              @contextmenu.prevent.right="showDropdownMenu(message)"
            >
              <span v-if="message.sender !== userName" class="sender-name"
                >{{ message.sender }}:</span
              >
              {{ message.text }}

              
              <div
                v-if="showDropdownMenuForMessage === message.id"
                class="dropdown"
              >
                <button @click="removeMessageForAll(message.id)">
                  Удалить
                </button>
                <button @click="editMessage(message)">Редактировать</button>
              </div>

              <!-- <button
                v-if="message.sender === userName" 
                @click="removeMessageForAll(message.id)"
              >
                Remove for All
              </button>
              <button
                v-if="message.sender === userName"
                @click="editMessage(message)"
              >
                Edit
              </button> -->
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
            <button @click="loadAvailableGroups">Back to Menu</button>
            <button @click="leaveGroup">Leave Group</button>
          </div>
        </div>
        <div class="input-container" v-if="isEditing">
          <!-- Editing mode -->
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

.system-message{
  text-align: center;
  color: #ab2424;

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
</style>
