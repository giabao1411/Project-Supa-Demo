
import { apiFetch, LogOut } from "./JsCommon.js";
import  ChoicesHelper  from "./choicesHelper.js";
import CONFIG from "./config.js";
const APIAuth = CONFIG.API_AUTH;
const APIAdmin = CONFIG.API_ADMIN;
let id;
let isEdit=false;
initAdminCheck();
  await ChoicesHelper.bindApi("#selPermissions",`${APIAdmin}/permissions`);
const modalRole = new Modal(document.getElementById("divRoleModal"),{
 onClose: function () {
            clearEditModal();
        },
        onHide: function () {
            clearEditModal();
        }
});
function clearEditModal(){
    document.getElementById('txtRoleName').value="";
   document.getElementById('txtDescription').value="";
    ChoicesHelper.clear("#selPermissions");
    isEdit=false;
    id=null;
}
document.getElementById("btnLogOut").addEventListener("click", function(e){
    e.preventDefault();
    LogOut
})
document.getElementById('btnAddNewRole').addEventListener("click",async function(){
    bindTextPopup("Add New Role","Create Role");
   
 modalRole.show();

});
document.getElementById('btnCreateRole').addEventListener("click", async function (e){
    e.preventDefault();
    
    const txtRoleName = document.getElementById('txtRoleName').value;
    const txtDescription = document.getElementById('txtDescription').value;
    const listPermissions= ChoicesHelper.getValue("#selPermissions");
    if(!isEdit){

        await CreateRole(txtRoleName,txtDescription,listPermissions);
    }
    else{
        await UpdateRole(id,txtRoleName,txtDescription,listPermissions);
    }

})
document.addEventListener('click', async function (event) {
    //  await ChoicesHelper.bindApi("#selPermissions",`${APIAdmin}/permissions`);
        const btnDelete = event.target.closest(".btnDeleteRole");
        const btnEdit = event.target.closest(".btnEditRole");
        if (btnDelete) {
            id = btnDelete.dataset.id;
            modal.show()

        } if (btnEdit) {
            id = btnEdit.dataset.id;
            await bindEditRole(id);
            modalRole.show();
        }
    });
document.getElementById('btnCancelRoleView').addEventListener("click",function(){
    modalRole.hide();
});
document.getElementById('btnCloseRoleView').addEventListener("click",function (){
    modalRole.hide();
})
const modal = new Modal(document.getElementById("deleteModal"));
 document.getElementById("btnCancelDelete").addEventListener("click", async function (e) {
        modal.hide();
    });
    document.getElementById("btnConfirmDelete").addEventListener("click", async function (e) {
        
        await DeleteRole(id);
        modal.hide();
    });

    function bindTextPopup(title,nameButton){
    document.getElementById("divTitleRole").querySelector("h3").innerText=title;
    document.getElementById("btnCreateRole").innerText=nameButton;
}
async function bindEditRole(id){
    isEdit = true;
       bindTextPopup("Update Role","Update Role");
        const res = await apiFetch(`${APIAdmin}/roles-by-id`, {
            method: "POST",
            credentials: 'include',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(id)
        });
        if (res.ok) {
            const role = await res.json();
    
            document.getElementById("txtRoleName").value = role.name;
            document.getElementById("txtDescription").value = role.description;
            ChoicesHelper.setValue("#selPermissions", role.permissionIds)
    
        }
}
async function DeleteRole(id){
    const res = await apiFetch(`${APIAdmin}/roles-delete`, {
                method: "DELETE",
                credentials: 'include',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(id)
        
            });
            if (res.ok) {
                alert("User deleted successfully");
                LoadDataRole();
            } else {
                alert("Failed to delete user");
                LoadDataRole();
            }
}
    
async function UpdateRole(id,name,des,permissions){
const resEdit = await apiFetch(`${APIAdmin}/roles-update`, {
            method: "PUT",
            credentials: 'include',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
               Id: id,
               Name: name,
               Description: des,
               PermissionIds: permissions
            })
        });
        if (resEdit.ok) {
            alert("User updated successfully");
            LoadDataRole();
            modalRole.hide();
        } else {
            alert("Failed to update user");
             LoadDataRole();
            modalRole.hide();
        }
}    

async function CreateRole(name,des,permissions){
     const res = await apiFetch(`${APIAdmin}/roles-create`,{
            method : "POST",
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                Name: name,
                Description : des,
                PermissionIds: permissions
            }),
    
        });
        if(res.ok){
            console.log(res.json())
            modalRole.hide();
            LoadDataRole();
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
         if(user.avatarUrl!=null){
            document.getElementById("imgAvatar").src=user.avatarUrl;
        }
        document.getElementById("lblUserName").innerText=user.email;
        document.getElementById("lblRole").innerText=user.roles[0].name;
       LoadDataRole();
    }
}
async function LoadDataRole() {
    const tbRole = document.getElementById("tbdRole");
    const res = await apiFetch(`${APIAdmin}/roles`, {
        method: "GET",
        header: { 'Content-Type': 'application/json' }
    })
    if (res.ok) {
        const data = await res.json();
        bindTableRole(data, tbRole);
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
    data.forEach(role => {
        const row = `<tr>
<td class="px-6 py-4 font-bold text-slate-900 dark:text-white">${role.name}</td>
<td class="px-6 py-4 text-sm text-slate-500">${role.description}</td>
<td class="px-6 py-4 text-right">
<div class="flex justify-end gap-2 text-slate-400">
<button data-id="${role.id}" class="hover:text-primary transition-colors btnEditRole"><span class="material-symbols-outlined text-lg ">edit</span></button>
<button data-id="${role.id}" class="hover:text-red-500 transition-colors btnDeleteRole"><span class="material-symbols-outlined text-lg ">delete</span></button>
</div>
</td>
</tr>`;
        tableBody.innerHTML += row;
    });

}