import CONFIG from "./config.js";
import { apiFetch, LogOut } from "./JsCommon.js";
const APIAuth = CONFIG.API_AUTH;
const APIAdmin = CONFIG.API_ADMIN;
let id;
let isEdit=false;
initAdminCheck();
const modalPermission = new Modal(document.getElementById("divPermissionModal"),{
 onClose: function () {
            clearEditModal();
        },
        onHide: function () {
            clearEditModal();
        }
});
const modal = new Modal(document.getElementById("deleteModal"));
 document.getElementById("btnCancelDelete").addEventListener("click", async function (e) {
        modal.hide();
    });
    document.getElementById("btnConfirmDelete").addEventListener("click", async function (e) {
        
        await DeletePermission(id);
        modal.hide();
    });
document.getElementById("addNewPermission").addEventListener("click", () => {
    bindTextPopup("Add New Permission","Create Permission");
    modalPermission.show();
});
document.getElementById("btnLogOut").addEventListener("click",function (e){
    e.preventDefault();
    LogOut();
})
document.getElementById("btnCancelPermissionView").addEventListener("click", () => { 
    modalPermission.hide();
});
document.getElementById("btnClosePermissionView").addEventListener("click",() =>{
    modalPermission.hide();
} )
document.addEventListener('click', async function (event) {
        const btnDelete = event.target.closest(".btnDeletePermission");
        const btnEdit = event.target.closest(".btnEditPermission");
        if (btnDelete) {
            id = btnDelete.dataset.id;
            modal.show()

        } if (btnEdit) {
            id = btnEdit.dataset.id;
            await bindEditPermission(id);
            modalPermission.show();
        }
    });
document.getElementById("btnCreatePermission").addEventListener("click",async function(e) {
    const txtName= document.getElementById("txtPermissionName").value;
    const txtDescription = document.getElementById("txtDescription").value;
    if(!isEdit){
  await CreatePermission(txtName,txtDescription);
    }
   
    await UpdatePermission(id,txtName,txtDescription);
}

);
async function DeletePermission(id){
    const res = await apiFetch(`${APIAdmin}/permissions-delete`, {
            method: "DELETE",
            credentials: 'include',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(id)
    
        });
        if (res.ok) {
            alert("User deleted successfully");
            LoadDataPermission();
        } else {
            alert("Failed to delete user");
            LoadDataPermission();
        }
}
async function UpdatePermission(id,name,des){
    const resEdit = await apiFetch(`${APIAdmin}/permissions-update`, {
            method: "PUT",
            credentials: 'include',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
               Id: id,
               Name: name,
               Description: des,
            })
        });
        if (resEdit.ok) {
            alert("User updated successfully");
            LoadDataPermission();
            modalPermission.hide();
        } else {
            alert("Failed to update user");
             LoadDataPermission();
            modalPermission.hide();
        }
}
async function CreatePermission(name,des){
    const res = await apiFetch(`${APIAdmin}/permissions-create`,{
        method : "POST",
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            Name: name,
            Description : des,
        }),

    });
    if(res.ok){
        console.log(res.json())
        modalPermission.hide();
        LoadDataPermission();
    }
}
async function initAdminCheck() {

    const res = await apiFetch(`${APIAuth}/me`, {
        method: 'GET',
        
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
        document.getElementById("contentSettingView").style.display = "block";
         if(user.avatarUrl!=null){
            document.getElementById("imgAvatar").src=user.avatarUrl;
        }
        document.getElementById("lblUserName").innerText=user.email;
        document.getElementById("lblRole").innerText=user.roles[0].name;
       LoadDataPermission();
    }
}
async function LoadDataPermission() {
    const tbPermission = document.getElementById("tbdPermission");
    const res = await apiFetch(`${APIAdmin}/permissions`, {
        method: "GET",
        header: { 'Content-Type': 'application/json' }
    })
    if (res.ok) {
        const data = await res.json();
        bindTableRole(data, tbPermission);
    } else {
        window.location.href = "./Error.html";
    }

}
async function bindTableRole(data, tableBody) {
    tableBody.innerHTML="";
    if (data.length === 0) {
        tableBody.innerHTML = '<tr><td colspan="3" class="px-6 py-4 text-center text-slate-500">No roles found.</td></tr>';
        return;
    }
    data.forEach(permission => {
        const row = `<tr>
<td class="px-6 py-4 font-bold text-slate-900 dark:text-white">${permission.name}</td>
<td class="px-6 py-4 text-sm text-slate-500">${permission.description}</td>
<td class="px-6 py-4 text-right">
<div class="flex justify-end gap-2 text-slate-400">
<button data-id="${permission.id}" class="hover:text-primary transition-colors btnEditPermission"><span class="material-symbols-outlined text-lg ">edit</span></button>
<button data-id="${permission.id}" class="hover:text-red-500 transition-colors btnDeletePermission"><span class="material-symbols-outlined text-lg ">delete</span></button>
</div>
</td>
</tr>`;
        tableBody.innerHTML += row;
    });

}
function bindTextPopup(title,nameButton){
    document.getElementById("divTitlePermission").querySelector("h3").innerText=title;
    document.getElementById("btnCreatePermission").innerText=nameButton;
}
async function bindEditPermission(id) {
    
    isEdit = true;
   bindTextPopup("Update Permission","Update Permission");
    const res = await apiFetch(`${APIAdmin}/permissions-by-id`, {
        method: "POST",
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(id)
    });
    if (res.ok) {
        const permission = await res.json();

        document.getElementById("txtPermissionName").value = permission.name;
        document.getElementById("txtDescription").value = permission.description;

    }
};
function clearEditModal() {
    document.getElementById('txtPermissionName').value = '';
    document.getElementById('txtDescription').value = '';
   
    isEdit = false;
    id = null;
}
