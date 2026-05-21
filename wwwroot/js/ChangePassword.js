import { apiFetch } from "./JsCommon.js";
import CONFIG from "./config.js";
const API = CONFIG.API_AUTH;

const changePasswordBtn = document.getElementById('btnUpdatePassword');
changePasswordBtn.addEventListener('click', function (event) {
    event.preventDefault();
    updatePassword();
})
async function updatePassword(){
   const currentPassword = document.getElementById('txtCurrentPassword').value;
   const newPassword = document.getElementById('txtNewPassword').value;
   const confirmPassword = document.getElementById('txtConfirmPassword').value;
   if(newPassword !== confirmPassword){
    alert("New password and confirm password do not match");
    return;
   }
   const res= await apiFetch(`${API}/update-user-password`, {
    method: 'PUT',
   
    headers: {
        'Content-Type': 'application/json'
    },
    body: JSON.stringify({
        currentPassword:currentPassword,
        password: newPassword,
        coffirmPassword: confirmPassword,
    })
   });
    if(res.ok){
        const data = await res.text();
        alert(data);
        window.location.href = "\Home.html";
    }else{
        const error = await res.text();
        alert(error);
        window.location.href = '/Error.html';
    }
}
