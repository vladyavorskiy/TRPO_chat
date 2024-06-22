<script setup lang="ts">
import axios from "axios";
import { ref, watch, onMounted, computed } from "vue";
import { HubConnectionBuilder, HubConnection } from "@microsoft/signalr";
import { nextTick } from "vue";
import {
  ElInput,
  ElButton,
  ElIcon,
  ElTooltip,
  ElContainer,
  ElAside,
  ElHeader,
  ElMain,
  ElText,
  ElDropdown,
  ElDropdownItem,
  ElDropdownMenu,
  ElDivider,
  ElLoading,
  ElMessage,
  ElDialog,
  ElRadio,
  ElRadioGroup,
  ElRadioButton,
} from "element-plus";
import "element-plus/dist/index.css";
import { FailedToStartTransportError } from "@microsoft/signalr/dist/esm/Errors";

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

interface UsersInfo {
  name: string;
  role: string;
}

const userNameInput = ref("");
const userName = ref("");
const currentGroup = ref<string>("");
const newMessage = ref("");
const newGroupName = ref<string>("");
const availableGroups = ref<string[]>([]);
const groupMembers = ref<UsersInfo[]>([]);
const groupMessages = ref<Message[]>([]);
const searchQuery = ref<string>("");
const isSearching = ref(false); // Added search mode toggle
const dialogUserVisible = ref(false);
const dialogRoleVisible = ref(false);
const selectedRole = ref<string>("");
const selectedMembertoChangeRole = ref<string>("");
//const isGroupCreator = ref(false);
const isGroupRole = ref<string>("");
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

  connection.value.on("onMessage", (message: Message) => {
    if (currentGroup.value) {
      groupMessages.value.push(message);
    }
    scrollToEnd();
  });

  connection.value.on("onUserJoin", (userName: string, id: number) => {
    if (currentGroup.value) {
      const message = `${userName} вступил в группу.`;
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
      const message = `${userName} покинул группу.`;
      groupMessages.value.push({
        text: message,
        sender: "System",
        id: id,
        replyTo: null,
      });
      scrollToEnd();
    }
  });

  connection.value.on(
    "onUserDeleted",
    (groupName: string, userNameDeleted: string, id: number) => {
      if (currentGroup.value) {
        const message = `${userNameDeleted} был удален администрацией.`;
        groupMessages.value.push({
          text: message,
          sender: "System",
          id: id,
          replyTo: null,
        });
        scrollToEnd();
        if (
          currentGroup.value === groupName &&
          userNameDeleted === userName.value
        ) {
          currentGroup.value = "";
          groupMessages.value = [];
          availableGroups.value = availableGroups.value.filter(
            (group) => group !== groupName
          );
        }
      }
    }
  );

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

  connection.value.on(
    "onUserRoleEdited",
    (groupName: string, userNameNewRole: string, newRole: string) => {
      if (currentGroup.value === groupName) {
        const user = groupMembers.value.find(
          (user) => user.name === userNameNewRole
        );
        if (user) {
          user.role = newRole;
        }
        if (userName.value === userNameNewRole) {
          isGroupRole.value = newRole;
        }
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
    var container = document.querySelector(".chat_container");
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

const joinGroup = async (group: string) => {
  searchQuery.value = "";
  isSearching.value = false;
  if (userName.value) {
    try {
      await axios.post("/api/Chat/JoinGroup", null, {
        params: { groupName: group, userName: userName.value },
      });
      currentGroup.value = group;
      await loadGroupMessages(group);
      if (!availableGroups.value.includes(group)) {
        availableGroups.value.push(group);
      }
      const response = await axios.get("/api/Chat/GetUsersRole", {
        params: { groupName: group, userName: userName.value },
      });
      isGroupRole.value = response.data;
      newGroupName.value = "";
      nextTick(() => {
        const inputElement = document.getElementById("eli-message");
        if (inputElement) {
          inputElement.focus();
        }
      });
    } catch (error) {
      console.error("Error joining group:", error);
      ElMessage.error("Ошибка при вступлении в чат");
      ElMessage({
        message: "Возможно, группа с таким названием не существует",
        type: "warning",
      });
    }
  }
};

const createGroup = async () => {
  searchQuery.value = "";
  isSearching.value = false;
  if (newGroupName.value.trim() !== "") {
    try {
      await axios.post("/api/Chat/CreateGroup", null, {
        params: { groupName: newGroupName.value, creator: userName.value },
      });
      //availableGroups.value.push(newGroupName.value);
      await joinGroup(newGroupName.value);
      newGroupName.value = "";
      nextTick(() => {
        const inputElement = document.getElementById("eli-message");
        if (inputElement) {
          inputElement.focus();
        }
      });
    } catch (error) {
      console.error("Error creating group:", error);
      ElMessage.error("Ошибка при создании чата");
      ElMessage({
        message: "Возможно, группа с таким названием уже существует",
        type: "warning",
      });
    }
  }
};

const deleteGroup = async () => {
  searchQuery.value = "";
  isSearching.value = false;
  if (currentGroup.value && userName.value) {
    try {
      const response = await axios.post("/api/Chat/DeleteGroup", null, {
        params: { groupName: currentGroup.value, userName: userName.value },
      });
      console.log("Response from server:", response.data);
      currentGroup.value = "";
      groupMessages.value = [];
      nextTick(() => {
        const inputElement = document.getElementById("eli-create");
        if (inputElement) {
          inputElement.focus();
        }
      });
      isGroupRole.value = "";
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
  nextTick(() => {
    const inputElement = document.getElementById("eli-message");
    if (inputElement) {
      inputElement.focus();
    }
  });
};

const sendMessage = async () => {
  searchQuery.value = "";
  isSearching.value = false;
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
  await nextTick();
  const container_load = document.querySelector(".main_container");
  const loading = ElLoading.service({
    target: container_load,
    lock: true,
    text: "Loading",
    background: "rgba(255, 255, 255, 0.8)",
  });
  try {
    const response = await axios.get("/api/Chat/GetGroupMessages", {
      params: { groupName: group },
    });
    groupMessages.value = response.data;
    scrollToEnd();
  } catch (error) {
    console.error("Error loading group messages:", error);
  } finally {
    setTimeout(() => {
      loading.close();
    }, 1000);
  }
};

const closeGroup = async () => {
  currentGroup.value = "";
  groupMessages.value = [];
  isGroupRole.value = "";
  nextTick(() => {
    const inputElement = document.getElementById("eli-create");
    if (inputElement) {
      inputElement.focus();
    }
  });
};

const loadAvailableGroups = async () => {
  currentGroup.value = "";
  if (userName.value) {
    try {
      const response = await axios.get("/api/Chat/GetUserGroups", {
        params: { userName: userName.value },
      });
      availableGroups.value = response.data;
      nextTick(() => {
        const inputElement = document.getElementById("eli-create");
        if (inputElement) {
          inputElement.focus();
        }
      });
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

const showDropdownMenuForMessage = ref<number | null>(null);

const showDropdownMenu = (message) => {
  if (message.sender == "System") {
    showDropdownMenuForMessage.value = null;
  } else if (showDropdownMenuForMessage.value === message.id) {
    showDropdownMenuForMessage.value = null;
  } else {
    showDropdownMenuForMessage.value = message.id;
  }
};

const editMessage = async (message) => {
  isEditing.value = true;
  editedMessage.value = message.text;
  editedMessageId = message.id;
  nextTick(() => {
    const inputElement = document.getElementById("eli-edit");
    if (inputElement) {
      inputElement.focus();
    }
  });
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
      nextTick(() => {
        const inputElement = document.getElementById("eli-message");
        if (inputElement) {
          inputElement.focus();
        }
      });
    } catch (error) {
      console.error("Error editing message:", error);
    }
  }
};

const scrollToOriginalMessage = (message) => {
  console.log("scrollToOriginalMessage:");
  if (message.replyTo !== null) {
    console.log(
      "Original message ID:",
      `message-${message.replyTo.messageReplyId}`
    );
    const originalMessage = groupMessages.value.find(
      (msg) => msg.id === message.replyTo.messageReplyId
    );
    console.log("Original message:", originalMessage);
    if (originalMessage) {
      nextTick(() => {
        const originalMessageElement = document.querySelector(
          `[data-id='message-${originalMessage.id}']`
        );
        console.log("Original message element:", originalMessageElement);
        if (originalMessageElement) {
          originalMessageElement.scrollIntoView({
            behavior: "smooth",
            block: "center",
          });
          originalMessageElement.classList.add("highlight");
          setTimeout(() => {
            originalMessageElement.classList.remove("highlight");
          }, 1000);
        }
      });
    }
  }
};

const toggleSearchMode = () => {
  isSearching.value = !isSearching.value;
  searchQuery.value = "";
  if (isSearching.value) {
    nextTick(() => {
      const inputElement = document.getElementById("eli-search");
      if (inputElement) {
        inputElement.focus();
      }
    });
  } else {
    nextTick(() => {
      const inputElement = document.getElementById("eli-message");
      if (inputElement) {
        inputElement.focus();
      }
    });
  }
};

const filteredMessages = computed(() => {
  if (currentGroup.value) {
    if (searchQuery.value.trim() === "") {
      return groupMessages.value;
    } else if (isSearching.value) {
      return groupMessages.value.filter(
        (message) =>
          message.sender !== "System" &&
          message.text.toLowerCase().includes(searchQuery.value.toLowerCase())
      );
    } else {
      return groupMessages.value.filter((message) =>
        message.text.toLowerCase().includes(searchQuery.value.toLowerCase())
      );
    }
  }
});

const loadGroupMembers = async (group) => {
  try {
    const response = await axios.get("/api/Chat/GetUsersInGroup", {
      params: { groupName: group },
    });
    groupMembers.value = response.data;
    dialogUserVisible.value = true;
  } catch (error) {
    console.error("Error loading group members:", error);
  }
};

const isCurrentUserAdminOrCreator = computed(() => {
  const currentUser = groupMembers.value.find(
    (member) => member.name === userName.value
  );
  return (
    currentUser &&
    (currentUser.role == "Admin" || currentUser.role == "Creator")
  );
});

const leaveGroup = async () => {
  searchQuery.value = "";
  isSearching.value = false;
  if (currentGroup.value && userName.value) {
    try {
      const response = await axios.post("/api/Chat/LeaveGroup", null, {
        params: { groupName: currentGroup.value, userName: userName.value },
      });
      console.log("Response from server:", response.data);
      availableGroups.value = availableGroups.value.filter(
        (group) => group !== currentGroup.value
      );
      currentGroup.value = "";
      groupMessages.value = [];
      nextTick(() => {
        const inputElement = document.getElementById("eli-create");
        if (inputElement) {
          inputElement.focus();
        }
      });
      isGroupRole.value = "";
      //await loadAvailableGroups();
    } catch (error) {
      console.error(
        "Error leaving group:",
        error.response ? error.response.data : error.message
      );
    }
  }
};

const removeUserFromGroup = async (memberName: string) => {
  try {
    await axios.post("/api/Chat/DeleteUser", null, {
      params: { groupName: currentGroup.value, userName: memberName },
    });

    groupMembers.value = groupMembers.value.filter(
      (member) => member.name !== memberName
    );
    ElMessage.success(`Пользователь ${memberName} успешно удален`);
  } catch (error) {
    console.error("Error removing user from group:", error);
    ElMessage.error(`Ошибка при удалении пользователя ${memberName}`);
  }
};

const formatRole = (role) => {
  if (role === "Common") {
    return "Участник";
  } else if (role === "Admin") {
    return "Админ";
  } else if (role === "Moderator") {
    return "Модер";
  } else if (role === "Creator") {
    return "Создатель";
  }
  return role;
};

onMounted(async () => {
  if (userName.value) {
    await initializeSignalRConnection();
    await setUserConnection();
    await loadAvailableGroups();
  }
});

const closeDialogRole = async () => {
  dialogRoleVisible.value = false;
  selectedRole.value = "";
  selectedMembertoChangeRole.value = "";
};

const openDialogRole = async (memberName: string) => {
  try {
    const response = await axios.get("/api/Chat/GetUsersRole", {
      params: { groupName: currentGroup.value, userName: memberName },
    });
    selectedRole.value = response.data;
    dialogRoleVisible.value = true;
    selectedMembertoChangeRole.value = memberName;
  } catch (error) {
    console.error("Error", error);
    ElMessage.error(`Ошибка кто`);
  }
};

const changeUserRole = async () => {
  try {
    const response = await axios.post("/api/Chat/ChangeUserRole", null, {
      params: {
        groupName: currentGroup.value,
        userName: selectedMembertoChangeRole.value,
        newRole: selectedRole.value,
      },
    });
    dialogRoleVisible.value = false;
    selectedRole.value = "";
    selectedMembertoChangeRole.value = "";
  } catch (error) {
    console.error("Error", error);
    ElMessage.error(`Ошибка что`);
  }
};
</script>

<template>
  <div v-if="!userName" class="name_container">
    <el-input
      v-model="userNameInput"
      placeholder="Введите имя"
      style="width: 240px"
      size="large"
      maxlength="15"
      show-word-limit
      clearable
    />
    <el-button color="#41B3A3" size="large" type="primary" @click="setUserName"
      >Войти</el-button
    >
  </div>

  <div v-else>
    <el-container class="common_layout">
      <el-dialog v-model="dialogRoleVisible" title="Роли" width="500">
        <el-radio-group v-model="selectedRole">
          <el-radio-button label="Участник" value="Common" />
          <el-tooltip
            content="Может удалять людей и сообщения"
            placement="top"
            effect="customized"
          >
            <el-radio-button label="Модер" value="Moderator" />
          </el-tooltip>
          <el-tooltip
            content="Может удалять сообщения"
            placement="top"
            effect="customized"
          >
            <el-radio-button label="Админ" value="Admin" />
          </el-tooltip>
        </el-radio-group>
        <template #footer>
          <div class="dialog-footer">
            <el-button @click="closeDialogRole()">Закрыть</el-button>
            <el-button type="primary" @click="changeUserRole()"
              >Изменить роль</el-button
            >
          </div>
        </template>
      </el-dialog>

      <el-dialog v-model="dialogUserVisible" title="Участники" width="500">
        <div
          class="member-details"
          v-for="member in groupMembers"
          :key="member.name"
        >
          <div class="member-content">
            <div class="left-section">
              <div class="member-name">
                <span>{{ member.name }}</span>
                <el-tooltip
                  v-if="isGroupRole === 'Creator' || isGroupRole === 'Admin'"
                  content="Удалить участника"
                  placement="top"
                  effect="customized"
                >
                  <el-icon
                    v-if="member.role !== 'Creator'"
                    class="icon-spacing-dialog-left"
                    @click="removeUserFromGroup(member.name)"
                    :size="20"
                  >
                    <Close />
                  </el-icon>
                </el-tooltip>
              </div>
            </div>
            <div class="right-section">
              <div class="member-actions">
                <el-tooltip
                  v-if="isGroupRole === 'Creator' || isGroupRole === 'Admin'"
                  content="Изменить роль"
                  placement="top"
                  effect="customized"
                >
                  <el-icon
                    v-if="member.role !== 'Creator'"
                    class="icon-spacing-dialog-right"
                    @click="openDialogRole(member.name)"
                    :size="20"
                  >
                    <Operation />
                  </el-icon>
                </el-tooltip>
                <span>{{ formatRole(member.role) }}</span>
              </div>
            </div>
          </div>
        </div>
        <template #footer>
          <div class="dialog-footer">
            <el-button @click="dialogUserVisible = false">Закрыть</el-button>
          </div>
        </template>
      </el-dialog>

      <el-aside width="25%">
        <el-container class="left_container">
          <el-header class="create_header">
            <el-input
              id="eli-create"
              class="create_input"
              v-model="newGroupName"
              placeholder="Введите название чата"
            />
            <div class="button_container">
              <el-button
                class="create_buttons"
                color="#41B3A3"
                type="primary"
                @click="createGroup"
                >Создать</el-button
              >
              <el-button
                class="create_buttons"
                color="#41B3A3"
                type="primary"
                @click="joinGroup(newGroupName)"
              >
                Вступить
              </el-button>
            </div>
          </el-header>
          <el-main class="list_of_group">
            <ul class="group-list">
              <li v-for="group in availableGroups" :key="group">
                <el-button @click="joinGroup(group)">{{ group }} </el-button>
              </li>
            </ul>
          </el-main>
        </el-container>
      </el-aside>
      <el-divider
        direction="vertical"
        class="full-height-divider"
        border-style="dashed"
      />
      <el-container class="main_container">
        <el-header v-if="currentGroup" class="chat_header">
          <h2 class="chat-title">{{ currentGroup }}</h2>
          <el-input
            id="eli-search"
            v-if="isSearching"
            v-model="searchQuery"
            placeholder="Поиск сообщений"
            class="message-input search-input"
            clearable
          />
          <div class="buttons-container">
            <el-tooltip
              content="Посмотреть участников"
              effect="customized"
              placement="top"
            >
              <el-icon
                class="icon-spacing"
                @click="loadGroupMembers(currentGroup)"
                :size="30"
              >
                <User />
              </el-icon>
            </el-tooltip>

            <el-tooltip
              content="Поиск сообщения"
              effect="customized"
              placement="top"
            >
              <el-icon
                class="icon-spacing"
                :size="30"
                @click="toggleSearchMode"
              >
                <Search />
              </el-icon>
            </el-tooltip>

            <el-tooltip
              content="Закрыть группу"
              effect="customized"
              placement="top"
            >
              <el-icon class="icon-spacing" @click="closeGroup" :size="30">
                <Close />
              </el-icon>
            </el-tooltip>

            <el-tooltip
              content="Покинуть группу"
              effect="customized"
              placement="top"
            >
              <el-icon class="icon-spacing" @click="leaveGroup" :size="30">
                <Remove />
              </el-icon>
            </el-tooltip>

            <el-tooltip
              v-if="isGroupRole === 'Creator'"
              content="Удалить группу"
              placement="top"
              effect="customized"
            >
              <el-icon class="icon-spacing" @click="deleteGroup" :size="30">
                <Delete />
              </el-icon>
            </el-tooltip>
          </div>
        </el-header>
        <el-divider
          v-if="currentGroup"
          class="horizontal_divider"
          border-style="dashed"
        />
        <el-main class="chat_container">
          <div class="messages_container">
            <div
              v-for="message in filteredMessages"
              :key="message.id"
              :data-id="'message-' + message.id"
              :class="[
                'message',
                {
                  'user-message': message.sender === userName,
                  'other-message': message.sender !== userName,
                  'system-message': message.sender === 'System',
                  'choice-reply-message':
                    repliedMessageId == message.id && repliedMessageId != null,
                  'other-reply-to-message':
                    (message.replyTo &&
                      message.replyTo.replyState === false &&
                      message.sender == userName) ||
                    (message.replyTo &&
                      message.replyTo.replyState === true &&
                      message.sender != userName),
                  'user-reply-to-message':
                    (message.replyTo &&
                      message.replyTo.replyState === false &&
                      message.sender != userName) ||
                    (message.replyTo &&
                      message.replyTo.replyState === true &&
                      message.sender == userName),
                },
              ]"
              @contextmenu.prevent.right="showDropdownMenu(message)"
              @dblclick="scrollToOriginalMessage(message)"
            >
              <span v-if="message.sender !== userName && message.sender !== 'System'" class="sender-name">
                {{ message.sender }}:
              </span>
              {{ message.text }}

              <el-dropdown
                v-if="
                  message.sender !== 'System' &&
                  (message.replyTo?.replyState === null ||
                    message.replyTo === null)
                "
                trigger="contextmenu"
              >
                <span class="rotated">
                  <el-icon><More /></el-icon>
                </span>
                <template #dropdown>
                  <el-dropdown-menu>
                    <el-dropdown-item @click="replyMessage(message)"
                      >Ответить</el-dropdown-item
                    >
                    <el-dropdown-item
                      v-if="message.sender === userName"
                      @click="editMessage(message)"
                      >Изменить</el-dropdown-item
                    >
                    <el-dropdown-item
                      v-if="
                        message.sender === userName ||
                        isGroupRole === 'Creator' ||
                        isGroupRole === 'Admin' ||
                        isGroupRole === 'Moderator'
                      "
                      @click="removeMessageForAll(message.id)"
                      >Удалить</el-dropdown-item
                    >
                  </el-dropdown-menu>
                </template>
              </el-dropdown>
            </div>
          </div>
          <div class="input-container" v-if="!isEditing && currentGroup">
            <el-input
              id="eli-message"
              class="message-input"
              v-model="newMessage"
              placeholder="Напишите сообщение..."
              @keyup.enter="sendMessage"
              size="large"
            />
            <el-button
              class="message-buttons"
              type="submit"
              size="large"
              @click="sendMessage"
              >Отправить</el-button
            >
          </div>
          <div class="input-container" v-if="isEditing && currentGroup">
            <el-input
              id="eli-edit"
              class="message-input"
              v-model="editedMessage"
              @keydown.enter="saveEdit"
              @keydown.escape="cancelEdit"
              placeholder="Edit your message..."
              size="large"
            />
            <el-button class="message-buttons" size="large" @click="saveEdit"
              >Сохранить</el-button
            >
            <el-button class="message-buttons" size="large" @click="cancelEdit"
              >Отмена</el-button
            >
          </div>
        </el-main>
      </el-container>
    </el-container>
  </div>
</template>

<style>
.template {
  box-sizing: border-box;
}
*,
*::after,
*::before {
  box-sizing: inherit;
}

.name_container {
  display: flex;
  justify-content: center;
  align-items: center;
  height: 100vh;
}

.common_layout {
  display: flex;
  width: 100%;
  height: 94vh;
}

.left_container {
  position: fixed;
  top: 0;
  left: 0;
  width: 25%;
  margin-right: 10px;
  margin-top: 30px;
  border: 1px 1px 1px 1px;
}

.create_header {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  /* align-items: center; */
  padding: 10px;
  margin-bottom: 15px;
}

.create_input {
  width: 100%; /* Ширина на всю доступную область */
  margin-bottom: 10px; /* Отступ снизу для разделения с кнопками */
}

.button_container {
  display: flex;
  width: 100%;
}

.create_buttons {
  flex: 1; /* Равное распределение места */
  margin: 0 5px; /* Отступы между кнопками */
}

.create_buttons:first-child {
  margin-left: 0; /* Убираем отступ слева у первой кнопки */
}

.create_buttons:last-child {
  margin-right: 0; /* Убираем отступ справа у последней кнопки */
}

.list_of_group {
  padding: 10px;
}

.group-list {
  list-style: none;
  padding: 0;
}

.group-list button {
  background-color: #ffffff;
  color: #000000;
  border: none;
  padding: 10px 15px;
  cursor: pointer;
  width: 100%;
  font-size: larger;
  text-align: left;
  display: block;
}

.group-list button:hover {
  background-color: #7acabf;
  text-decoration: none;
}

.main_container {
  margin-right: 10px;
  margin-top: 20px;
  overflow: hidden;
  flex: 1;
  display: flex;
  flex-direction: column;
}

.chat_header {
  display: flex;
  align-items: center;
  padding: 10px;
}

.chat-title {
  flex: 1;
  margin-right: 5px;
}

.input-container {
  display: flex;
  align-items: center;
  padding: 10px;
  position: fixed;
  width: calc(100% - 30%);
  bottom: 0;
  background-color: #ffffff;
}

.message-input {
  flex: 1;
  margin-right: 5px;
}

.message-buttons {
  margin-left: 5px;
}

.icon-spacing {
  margin-right: 10px;
}

.el-popper.is-customized,
.el-popper__arrow::before {
  background: #84cdca;
}

.messages_container {
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.message {
  display: inline-block;
  padding: 10px;
  margin-bottom: 10px;
  border-radius: 5px;
  word-wrap: break-word;
  font-size: larger;
  background-color: #f1f1f1;
  text-align: center;
}

.user-message {
  background-color: #f0f0f0;
  color: #007bff;
  text-align: right;
  margin-left: auto;
}

.other-message {
  background-color: #57dc81;
  color: #000000;
  text-align: left;
  margin-right: auto;
}

.user-reply-to-message {
  text-align: right;
  background-color: #b8cdcdcc;
  margin-right: 20px;
  margin-left: auto;
}

.other-reply-to-message {
  text-align: left;
  background-color: #b8cdcdcc;
  margin-left: 20px;
  margin-right: auto;
}

.highlight {
  background-color: #a94cd1cc;
}

.system-message {
  display: block;
  text-align: center;
  background-color: #ffffff;
  color: #000000;
  font-style: bold;
  margin: 0 auto;
}

.choice-reply-message {
  background-color: #32bbbbcc;
}

.full-height-divider {
  height: 100%;
}

.horizontal_divider {
  margin: 0;
}

.rotated {
  transform: rotate(90deg);
}

.member-details {
  margin-bottom: 20px; /* Добавим отступ между элементами списка */
}

.member-content {
  display: flex;
  justify-content: space-between; /* Распределяем left-section и right-section по краям */
  align-items: center; /* Выравниваем по вертикали */
}

.member-name {
  display: flex;
  align-items: center; /* Выравниваем элементы внутри member-name по вертикали */
}

.member-actions {
  display: flex;
  align-items: center; /* Выравниваем элементы внутри member-actions по вертикали */
}

.right-section {
  margin-right: 10px;
}

.left-section {
  margin-left: 10px;
}

.icon-spacing-dialog-right {
  margin-right: 10px;
}
.icon-spacing-dialog-left {
  margin-left: 7px;
}
</style>
