<script setup lang="ts">
import axios from "axios";
import { ref, computed } from "vue";
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
const isSearching = ref(false);
const dialogUserVisible = ref(false);
const dialogRoleVisible = ref(false);
const selectedRole = ref<string>("");
const selectedMembertoChangeRole = ref<string>("");
const isGroupRole = ref<string>("");
const connection = ref<HubConnection | null>(null);
const repliedMessageId = ref(null);
const editedMessage = ref("");
const isEditing = ref(false);
let editedMessageId = null;

const setUserName = async () => {
  if (userNameInput.value.trim() !== "") {
    userName.value = userNameInput.value.trim();
    await initializeSignalRConnection();
    await setUserConnection();
    await loadAvailableGroups();
  }
};

const initializeSignalRConnection = async () => {
  connection.value = new HubConnectionBuilder().withUrl("/hub").build();

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

const createGroup = async () => {
  searchQuery.value = "";
  isSearching.value = false;
  if (newGroupName.value.trim() !== "") {
    try {
      await axios.post("/api/Chat/CreateGroup", null, {
        params: { 
          createGroupName: newGroupName.value, 
          creatorName: userName.value },
      });
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

const joinGroup = async (group: string) => {
  searchQuery.value = "";
  isSearching.value = false;
  if (userName.value) {
    try {
      const responseFirst =await axios.post("/api/Chat/JoinGroup", null, {
        params: { 
          joinGroupName: group, 
          joinUserName: userName.value },
      });
      console.log("Response from server:", responseFirst.data);
      currentGroup.value = group;
      await loadGroupMessages(group);
      if (!availableGroups.value.includes(group)) {
        availableGroups.value.push(group);
      }
      const responseSecond = await axios.get("/api/Chat/GetUsersRole", {
        params: { 
          groupName: group, 
          userName: userName.value },
      });
      console.log("Response from server:", responseSecond.data);
      isGroupRole.value = responseSecond.data;
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


const leaveGroup = async () => {
  searchQuery.value = "";
  isSearching.value = false;
  if (currentGroup.value && userName.value) {
    try {
      const response = await axios.post("/api/Chat/LeaveGroup", null, {
        params: { 
          leaveGroupName: currentGroup.value, 
          leaveUserName: userName.value },
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
    } catch (error) {
      console.error("Error leaving group:", error);
    }
  }
};


const deleteGroup = async () => {
  searchQuery.value = "";
  isSearching.value = false;
  if (currentGroup.value && userName.value) {
    try {
      const response = await axios.post("/api/Chat/DeleteGroup", null, {
        params: { 
          deleteGroupName: currentGroup.value, 
          executorUserName: userName.value },
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
      const response = await axios.post("/api/Chat/SendMessage", null, { params });
      console.log("Response from server:", response.data);
      newMessage.value = "";
      repliedMessageId.value = null;
      scrollToEnd();
    } catch (error) {
      console.error("Error sending message:", error);
    }
  }
};

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
};

const saveEdit = async () => {
  if (editedMessageId !== null) {
    try {
      const response = await axios.post("/api/Chat/EditMessage", null, {
        params: {
          groupName: currentGroup.value,
          editMessageId: editedMessageId,
          newText: editedMessage.value,
        },
      });
      console.log("Response from server:", response.data);
      editedMessage.value = "";
      isEditing.value = false;
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

const removeMessageForAll = async (messageId) => {
  try {
    const response = await axios.post("/api/Chat/RemoveMessageForAll", null, {
      params: {
        groupName: currentGroup.value,
        removeMessageId: messageId,
      },
    });
    console.log("Response from server:", response.data);
  } catch (error) {
    console.error("Error remove message:", error);
  }
};


const removeUserFromGroup = async (memberName: string) => {
  try {
    const response = await axios.post("/api/Chat/DeleteUser", null, {
      params: { 
        groupName: currentGroup.value, 
        deleteUserName: memberName },
    });
    console.log("Response from server:", response.data);
    groupMembers.value = groupMembers.value.filter(
      (member) => member.name !== memberName
    );
    ElMessage.success(`Пользователь ${memberName} успешно удален`);
  } catch (error) {
    console.error("Error deleting user from group:", error);
    ElMessage.error(`Ошибка при удалении пользователя ${memberName}`);
  }
};


const changeUserRole = async () => {
  try {
    const response = await axios.post("/api/Chat/ChangeUserRole", null, {
      params: {
        groupName: currentGroup.value,
        changeRoleUserName: selectedMembertoChangeRole.value,
        newRole: selectedRole.value,
      },
    });
    console.log("Response from server:", response.data);
    dialogRoleVisible.value = false;
    ElMessage.success(`Роль пользователь ${selectedMembertoChangeRole.value} успешно изменена на ${selectedRole.value}`);
    selectedRole.value = "";
    selectedMembertoChangeRole.value = "";
  } catch (error) {
    console.error("Error change user role:", error);
    ElMessage.error(`Ошибка при изменении роли`);
  }
};


const loadAvailableGroups = async () => {
  currentGroup.value = "";
  if (userName.value) {
    try {
      const response = await axios.get("/api/Chat/GetUserGroups", {
        params: { userName: userName.value },
      });
      console.log("Response from server:", response.data);
      availableGroups.value = response.data;
      nextTick(() => {
        const inputElement = document.getElementById("eli-create");
        if (inputElement) {
          inputElement.focus();
        }
      });
    } catch (error) {
      console.error("Error loading user groups:", error);
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
    console.log("Response from server:", response.data);
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


const loadGroupMembers = async (group) => {
  try {
    const response = await axios.get("/api/Chat/GetUsersInGroup", {
      params: { groupName: group },
    });
    console.log("Response from server:", response.data);
    groupMembers.value = response.data;
    dialogUserVisible.value = true;
  } catch (error) {
    console.error("Error loading group members:", error);
  }
};


const openDialogRole = async (memberName: string) => {
  try {
    const response = await axios.get("/api/Chat/GetUsersRole", {
      params: { groupName: currentGroup.value, userName: memberName },
    });
    console.log("Response from server:", response.data);
    selectedRole.value = response.data;
    dialogRoleVisible.value = true;
    selectedMembertoChangeRole.value = memberName;
  } catch (error) {
    console.error("Error get user role", error);
  }
};

const closeDialogRole = async () => {
  dialogRoleVisible.value = false;
  selectedRole.value = "";
  selectedMembertoChangeRole.value = "";
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

const scrollToEnd = () => {
  setTimeout(() => {
    var container = document.querySelector(".chat_container");
    if (container !== null) {
      container.scrollTop = container.scrollHeight;
    } else {
      console.error("Element with class .chat_container not found");
    }
  }, 0);
};



const scrollToOriginalMessage = (message) => {
  console.log("scrollToOriginalMessage:");
  if (message.replyTo !== null) {
    console.log(
      "Original message ID:",
      `message-${message.replyTo.messageReplyId}`
    );
    const originalMessage = groupMessages.value.find(
      (msg) => msg.id === message.replyTo.messageReplyId && message.replyTo.replyState !== null
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
            content="Может удалять сообщения"
            placement="top"
            effect="customized"
          >
            <el-radio-button label="Модер" value="Moderator" />
          </el-tooltip>
          <el-tooltip
            content="Может удалять людей и сообщения"
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
          <div class="chat-title-container">
            <el-tooltip
              content="Закрыть группу"
              effect="customized"
              placement="top"
            >
              <el-icon class="icon-spacing" @click="closeGroup" :size="30">
                <Back />
              </el-icon>
            </el-tooltip>
            <h2 class="chat-title">{{ currentGroup }}</h2>
          </div>
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
              v-if="isGroupRole !== 'Creator'"
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
              @dblclick="scrollToOriginalMessage(message)"
            >
              <span
                v-if="
                  message.sender !== userName && message.sender !== 'System'
                "
                class="sender-name"
              >
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
              color="41B3A3"
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
  padding: 10px;
  margin-bottom: 15px;
}

.create_input {
  width: 100%; 
  margin-bottom: 10px; 
}

.button_container {
  display: flex;
  width: 100%;
}

.create_buttons {
  flex: 1; 
  margin: 0 5px; 
}

.create_buttons:first-child {
  margin-left: 0; 
}

.create_buttons:last-child {
  margin-right: 0; 
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
  color:#000
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
  justify-content: space-between;
  padding: 10px;
}

.chat-title {
  margin-left: 5px;
}

.chat-title-container {
  display: flex;
  align-items: center;
}

.buttons-container {
  display: flex;
  align-items: center;
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

.search-input {
  flex: 0.7;
  margin-right: 5px;
  margin-left: auto;
}

.message-buttons {
  margin-left: 5px;
  background-color: #41B3A3;
  border: 0;
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
  background-color: #E39078;
  color: #000;
  text-align: right;
  margin-left: auto;
}

.other-message {
  background-color: #D09FAF;
  color: #000000;
  text-align: left;
  margin-right: auto;
}

.user-reply-to-message {
  text-align: right;
  color: #000000;
  background-color: #DD6846;
  margin-right: 20px;
  margin-left: auto;
}

.other-reply-to-message {
  text-align: left;
  color: #000000;
  background-color: #B06980;
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
  margin-bottom: 20px; 
}

.member-content {
  display: flex;
  justify-content: space-between; 
  align-items: center; 
}

.member-name {
  display: flex;
  align-items: center; 
}

.member-actions {
  display: flex;
  align-items: center; 
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
