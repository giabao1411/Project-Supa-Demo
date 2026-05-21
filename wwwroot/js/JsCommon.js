import CONFIG from "./config.js";
const API = CONFIG.API_AUTH;
export async function refreshToken()
{
    let res = await fetch(`${API}/refresh`,{
        method: 'POST',
        credentials: 'include',

    });
    return res.ok;
}
export async function apiFetch (url , options = {}){
    options.credentials = 'include';
    let response = await fetch(url, options);
    if(response.status === 401){
        const refreshed = await refreshToken();
        if(!refreshed){
            // window.location.href = '/Login.html';
            return response;
        }
       response = await fetch(url, options);
    }
    
    return response;
}
export async function LogOut(){
    const res = await fetch(`${API}/logout`,{
        method: 'POST',
        credentials: 'include'
        });
        if(res.ok){
            alert("Logged out successfully");
            window.location.href = '/Home.html';
        }else{
            const error = await res.text();
            alert("Logout failed: " + error);
            window.location.href = '/Error.html';
        }
    
}