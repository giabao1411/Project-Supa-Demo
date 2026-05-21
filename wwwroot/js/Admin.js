import { apiFetch, LogOut } from "./JsCommon.js";
import { Pagination } from "./pagination.js";
import ChoicesHelper from "./choicesHelper.js"; 
import CONFIG from "./config.js";
const APIAdmin = CONFIG.API_ADMIN;
const APIAuth = CONFIG.API_AUTH;
const APIProduct = CONFIG.API_PRODUCT;
 const modal = new Modal(document.getElementById('deleteModal'));
    const modalEdit = new Modal(document.getElementById('addUserModal'), {
        onClose: function () {
            clearEditModal();
        },
        onHide: function () {
            clearEditModal();
        }

    });
initEvents();
initAdminCheck();
let files = [];
let filesEdit = [];
let isEdit = false;
let id = null;
let controller;
let debounceTimer;
let keyword = "";
let isSearching = false;
let pageSize = 10;
await ChoicesHelper.bindApi("#dropDownRoles",`${APIAdmin}/list-roles`);
const pagination = new Pagination({
    containerId: "divPagination",
    infoId: "divResultInfo",
    pageSize: pageSize,
    onPageChange: handlePageChange
});
// Event listeners

async function uploadImages(files) {
    const formData = new FormData();

    for (let file of files) {
        formData.append("files", file);
    }

    const res = await apiFetch(`${APIProduct}/upload`, {
        method: "POST",
        body: formData
    });

    if (!res.ok) throw new Error("Upload failed");

    return await res.json(); // trả về list url
}
async function initEvents() {
    document.getElementById("user-image").addEventListener("click", function () {
    this.value = null;
});

document.getElementById("user-image").addEventListener("change", function () {
    const preview = document.getElementById("preview");
    const input = document.getElementById("user-image");
    const divUpload = document.getElementById("divUpload");
    // preview.innerHTML = "";

    for (let file of this.files) {
       
        files.push(file);
        const div = document.createElement("div");
        div.className = "relative group aspect-[4/3]";

        div.innerHTML = `
        <img src="${URL.createObjectURL(file)}" class="w-full h-full object-contain"/>

        <button class="absolute top-1 right-1 bg-black/60 text-white rounded-full w-6 h-6 text-xs opacity-0 group-hover:opacity-100 transition">
          ✕
        </button>

        <div class="absolute bottom-0 left-0 w-full h-1 bg-gray-200 rounded">
          <div class="progress h-full bg-blue-500 w-0 rounded"></div>
        </div>
      `;
        // remove
        div.querySelector("button").onclick = () => {
            files = files.filter(f => f !== file);
            div.remove();
            divUpload.classList.toggle('hidden');
        };

        preview.appendChild(div);
    }
    divUpload.classList.add('hidden');
});
    document.getElementById("btnLogOut").addEventListener("click",function (e){
        e.preventDefault();
        LogOut();
    })
   
    document.getElementById("btnAddUser").addEventListener("click", function (e) {
        e.preventDefault();
        bindTitlePopupAdmin("Add New User","Fill in the details to create a new account.","Create User");
        modalEdit.show();
    });
    document.getElementById("btnCloseUserModal").addEventListener("click", function (e) {
        e.preventDefault();
        modalEdit.hide();
    });
    document.getElementById("btnCancelUserModal").addEventListener("click", function (e) {
        e.preventDefault();
        modalEdit.hide();
    });
    document.getElementById("txtSearch").addEventListener("input", function (e) {
        e.preventDefault();
        var roleId = document.getElementById("ddlRoles").value;
        keyword = e.target.value.trim();
        clearTimeout(debounceTimer);
        debounceTimer = setTimeout(() => {
            searchUser(e.target.value.trim(), roleId);
        }, 400);

    });
    document.getElementById("btnCancelDelete").addEventListener("click", async function (e) {
        modal.hide();
    });
    document.getElementById("btnConfirmDelete").addEventListener("click", async function (e) {
        const userId = document.getElementById("idUserTarget").value;
        await deleteUser(userId);
        modal.hide();
    });

    document.addEventListener('click', async function (event) {
        const btnDelete = event.target.closest(".btnDeleteUser");
        const btnEdit = event.target.closest(".btnEditUser");
        if (btnDelete) {
            const userId = btnDelete.dataset.id;
            document.getElementById("idUserTarget").value = userId;
            modal.show()

        } if (btnEdit) {
            const userId = btnEdit.dataset.id;
            await bindEditUser(userId);
            modalEdit.show();
        }
    });

    const btnCreateUser = document.getElementById("btnCreateUser");
    btnCreateUser.addEventListener("click", async function (e) {
        e.preventDefault();
        
        const txtUserName = document.getElementById("txtUserName").value;
        const txtEmail = document.getElementById("txtEmail").value;
        const txtPassword = document.getElementById("txtPassword").value;
        const txtConfirmPassword = document.getElementById("txtConfirmPassword").value;
        const dropDownRoles = ChoicesHelper.getValue("#dropDownRoles");
        if (txtPassword !== txtConfirmPassword) {
            alert("Passwords do not match");
            return;
        }
         let imageUrls = [];
        // console.log(files);
        if (files.length > 0) {
            imageUrls = await uploadImages(files);
           
        }
        if (!isEdit) {
            await CreateUser(txtUserName,txtEmail, txtPassword, txtConfirmPassword, dropDownRoles,imageUrls);
        } else {
            await UpdateUser(id, txtEmail, txtPassword, txtConfirmPassword, dropDownRoles,imageUrls);
        }
    });
}
async function initAdminCheck() {

    const res = await apiFetch(`${APIAuth}/me`, {
        method: 'GET',
        credentials: 'include',
        header: { 'Content-Type': 'application/json' }

    })
    if (!res.ok) {
        window.location.href = '/Error.html';
    } else {
        const user = await res.json();
        if (!user.permissions.includes("USER_UPDATE")) {
            // alert("You do not have permission to access this page");
            window.location.href = '/Error.html';
        }
        if(user.avatarUrl!=null){
            document.getElementById("imgAvatar").src=user.avatarUrl;
        }
        document.getElementById("lblUserName").innerText=user.email;
        document.getElementById("lblRole").innerText=user.roles[0].name;
        document.getElementById("contentAdminView").style.display = "block";
        LoadData();
    }
}
//
// Admin functions
async function CreateUser(txtUserName, txtEmail, txtPassword, txtConfirmPassword, dropDownRoles,imageUrl) {
    const rolesIs=dropDownRoles.map(id =>({
                    id: id
                }));
    const res = await apiFetch(`${APIAdmin}/create-users`, {
        method: 'POST',
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            userName: txtUserName,
            email: txtEmail,
            password: txtPassword,
            coffirmPassword: txtConfirmPassword,
            roles: rolesIs,
            avatarUrl: imageUrl[0]
        }),
    });
    if (res.ok) {
        alert("User created successfully");
        LoadData();
        modalEdit.hide();
    }
    else {
        alert("Failed to create user");
        LoadData();
        modalEdit.hide();
    }
}
async function UpdateUser(id, txtEmail, txtPassword, txtConfirmPassword, dropDownRoles,avatarUrl) {
    const rolesIs=dropDownRoles.map(id =>({
                    id: id
                }));
                
    const resEdit = await apiFetch(`${APIAdmin}/users-update`, {
        method: "PUT",
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            Id: id,
            email: txtEmail,
            password: txtPassword,
            coffirmPassword: txtConfirmPassword,
            roles: rolesIs,
            avatarUrl :avatarUrl[0]
        })
    });
    if (resEdit.ok) {
        alert("User updated successfully");
        LoadData();
        modalEdit.hide();
    } else {
        alert("Failed to update user");
        LoadData();
        modalEdit.hide();
    }

}
async function deleteUser(userId) {
    const res = await apiFetch(`${APIAdmin}/users-delete`, {
        method: "DELETE",
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(userId)

    });
    if (res.ok) {
        alert("User deleted successfully");
        LoadData();
    } else {
        alert("Failed to delete user");
        LoadData();
    }
}
async function searchUser(searchValue, roleId,page=1) {
    if (controller) {
        controller.abort();
    }

    controller = new AbortController();
    keyword = document.getElementById("txtSearch").value.trim();
    isSearching = true;
    const tableBody = document.getElementById('tblUsersBody');
    tableBody.innerHTML = '';
    if (searchValue === "") {
        keyword = "";
        isSearching = false;
        pagination.page = 1;
        LoadData();
        return;
    }
    const res = await apiFetch(`${APIAdmin}/users-search`, {
        method: "POST",
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            Keyword: searchValue,
            RoleId: roleId,
            page: page,
            pageSize: pageSize
        })
    });
    if (res.ok) {
        const users = await res.json();
        bindTable(users.items, tableBody);
        pagination.update(users.totalItems);
    } else {
        tableBody.innerHTML = '<tr><td colspan="6" class="px-6 py-4 text-center text-slate-500">No users found.</td></tr>';
    }
}
async function handlePageChange(page) {

    if (isSearching) {
        var roleId = document.getElementById("ddlRoles").value;
        await searchUser(keyword, roleId, page);

    } else {

        await LoadData(page);

    }
}
//
// Helper functions
function clearEditModal() {
    document.getElementById('txtEmail').value = '';
    document.getElementById('txtPassword').value = '';
    document.getElementById('txtConfirmPassword').value = '';
    document.getElementById('dropDownRoles').value = '';
    isEdit = false;
    id = null;
    files=[];
    filesEdit=[];
    ChoicesHelper.clear("#dropDownRoles");
}
function formatDate(dateString) {
    const date = new Date(dateString);
    const options = { month: 'short', day: 'numeric', year: 'numeric' };
    return date.toLocaleDateString("en-US", options);
}
//
// Data loading functions
async function LoadData(page=1) {
    document.getElementById('idUserTarget').value = "";
    const tableBody = document.getElementById('tblUsersBody');
    tableBody.innerHTML = '';
    const res = await apiFetch(`${APIAdmin}/list-users?page=${page}&pageSize=${pageSize}`, {
        method: 'GET',
        credentials: 'include',
        header: { 'Content-Type': 'application/json' }
    });
    if (res.ok) {
        const users = await res.json();
        bindTable(users.items, tableBody);
        pagination.update(users.totalItems);
    } else {
        window.location.href = '/Error.html';
    }
    // await getRoles('Select role', document.getElementById("dropDownRoles"));
    await getRoles('All Roles', document.getElementById("ddlRoles"), false);


}
async function bindTable(users, tableBody) {
    if (users.length === 0) {
        tableBody.innerHTML = '<tr><td colspan="6" class="px-6 py-4 text-center text-slate-500">No users found.</td></tr>';
        return;
    }

    users.forEach(user => {
        const row = `<tr class="hover:bg-slate-50/50 dark:hover:bg-slate-800/30 transition-colors group">
                                <td class="px-6 py-4">
                                 <input class="rounded text-primary focus:ring-primary/20 border-slate-300 dark:border-slate-700 bg-transparent" type="checkbox">
                                 </td>
                             <td class="px-6 py-4">
    <div class="flex items-center gap-3">
      <img class="w-10 h-10 rounded-full object-cover" data-alt="User avatar male beard" src="${user.avatarUrl?user.avatarUrl:'https://lh3.googleusercontent.com/aida-public/AB6AXuC1KN7gQyEVKH0nlnlWiv76sQCsFDr22rAZIm4hb1PSX-rM4IQPF9aJHOWzJfi5_Csy7x2aiI8j_qlxs5WUvWxtjUPwL8qVfKbPvfLbZRbfIE2sxT3YufV3D_OrLJYxdMVVLo7jXTfOk2fo_MRWEFhPdrtR8rK_DqBl1PL-psPTJ5GRZI7f8OWkK4ytJxqZH7plK-ZrqybuOAmD2eQecPKbMjRQddV0rmszci1QJnB49SfL0qnW5LutXlmQ2MlnT9Z-CTYXO_-hU7Jw'}">
<div>
  <p class="text-sm font-semibold text-slate-900 dark:text-white">${user.email}</p>
<p class="text-xs text-slate-500">${user.email}</p>
</div>
</div>
</td>
<td class="px-6 py-4">
<span class="px-2.5 py-1 text-xs font-medium rounded-full bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400">${user.roles.map(x => x.name).join(", ")}</span>

</td>
<td class="px-6 py-4">
 ${user.isEmailVerified ?
                `<span class="flex items-center text-xs font-medium text-emerald-600 dark:text-emerald-400">
    <span class="w-1.5 h-1.5 rounded-full bg-emerald-500 mr-2"></span>
   Active
  </span>`
                :
                `<span class="flex items-center text-xs font-medium text-amber-600 dark:text-amber-400">
<span class="w-1.5 h-1.5 rounded-full bg-amber-500 mr-2"></span>
Pending
</span>`
            }
 
  
            
</td>
<td class="px-6 py-4 text-sm text-slate-500">${formatDate(user.createdAt)}</td>
<td class="px-6 py-4 text-right">
<div class="flex items-center justify-end gap-2 opacity-0 group-hover:opacity-100 transition-opacity">
  <button id="btnEditUser" data-id="${user.id}" class="p-1.5 text-slate-400 hover:text-primary hover:bg-primary/5 rounded-md transition-all btnEditUser">
<span class="material-icons text-lg">edit</span>
</button>
<button id="btnCancelDelete" data-id="${user.id}" class="p-1.5 text-slate-400 hover:text-red-500 hover:bg-red-50 rounded-md transition-all btnDeleteUser">
  <span class="material-icons text-lg">delete</span>
</button>
<button data-modal-target="addUserModal" class="p-1.5 text-slate-400 hover:text-slate-600 dark:hover:text-white rounded-md transition-all">
                <span class="material-icons text-lg">more_vert</span>
                </button>
                    </div>
                        </td>
                                        </tr>`;
        tableBody.innerHTML += row;
    }
    )
}
async function getRoles(defaultValue, dropDownRoles, disable = true) {
    const resRoles = await apiFetch(`${APIAdmin}/list-roles`, {
        method: 'GET',
        credentials: 'include',
        header: { 'Content-Type': 'application/json' }
    });
    if (resRoles.ok) {
        var roles = await resRoles.json();

        dropDownRoles.innerHTML = `<option ${disable ? 'disabled="" selected="" ' : 'selected="" '} value="">${defaultValue}</option>`;
        roles.forEach(role => {
            const option = `<option value="${role.id}">${role.name}</option>`
            dropDownRoles.innerHTML += option;
        }
        )
    }
    else {
        return;
    }
}
function bindTitlePopupAdmin(titlePopup,description,nameBtn){
    document.getElementById("titlePopupAdminView").querySelector("h3").innerText=titlePopup;
    document.getElementById("titlePopupAdminView").querySelector("p").innerText=description;
    document.getElementById("btnCreateUser").innerText=nameBtn;
}
async function bindEditUser(userId) {
     bindTitlePopupAdmin("Update User","Fill in the details to update account.","Update User");
    document.getElementById("idUserTarget").value = userId;
    isEdit = true;
    id = userId;
    const res = await apiFetch(`${APIAdmin}/user-by-id`, {
        method: "POST",
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(userId)
    });
    if (res.ok) {
        const user = await res.json();

        document.getElementById("txtEmail").value = user.email;
        const rolesId =user.roles.map(x=>x.id);
        
        console.log(rolesId);
        ChoicesHelper.setValue("#dropDownRoles", rolesId)
        const preview = document.getElementById("preview");
    const input = document.getElementById("user-image");
    const divUpload = document.getElementById("divUpload");
    // preview.innerHTML = "";
    console.log(user.avatarUrl);
    if(user.avatarUrl===''||user.avatarUrl==null){
        return;
    }
        const div = document.createElement("div");
        div.className = "relative group aspect-[4/3]";

        div.innerHTML = `
        <img src="${user.avatarUrl}" class="w-full h-full object-contain"/>

        <button class="absolute top-1 right-1 bg-black/60 text-white rounded-full w-6 h-6 text-xs opacity-0 group-hover:opacity-100 transition">
          ✕
        </button>

        <div class="absolute bottom-0 left-0 w-full h-1 bg-gray-200 rounded">
          <div class="progress h-full bg-blue-500 w-0 rounded"></div>
        </div>
      `;
        // remove
        div.querySelector("button").onclick = () => {
           
            div.remove();
            divUpload.classList.toggle('hidden');
        };

        preview.appendChild(div);
    
    divUpload.classList.add('hidden');
        // document.getElementById("dropDownRoles").value = user.role.id;

    
};
}
//
