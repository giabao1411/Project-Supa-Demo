
import { apiFetch } from "./JsCommon.js";
import CONFIG from "./config.js";
const API = CONFIG.API_AUTH;
const APIProduct = CONFIG.API_PRODUCT;
const APICart = CONFIG.API_CART;
 initHome();
 initProduct();
 async function initProduct(){
    const res = await apiFetch(`${APIProduct}/products`);
    if(res.ok){
        const data = await res.json();
        renderProduct(data.items);
    }
 }
async function initHome() {
    const res = await apiFetch(`${API}/me`,{
        method: 'GET',
        credentials: 'include'
    })
    if(!res.ok){
       renderGuestHeader();
       return;
    }
    
    const user = await res.json();
    renderUserHeader(user);
     const cart = await apiFetch(`${APICart}`,{
            method: "GET",
        });
        
        const data = await cart.json();
        document.getElementById("lblTotalCartItem").innerText=data.items.length;
}
function renderGuestHeader() {
    const loginForm = `
    <button class="p-2 text-slate-600 dark:text-slate-300 hover:text-primary transition-colors" id="loginBtn">
<span class="material-icons-outlined">person_outline</span>
</button>
  `
  document.getElementById("clsLogin").insertAdjacentHTML('afterbegin',loginForm);
  
const loginBtn = document.getElementById('loginBtn');
    loginBtn.addEventListener('click', () => {
        window.location.href = '/Login.html';
    });
}

function renderUserHeader(user) {
    const layout =`<div class="relative group">
<button class="flex items-center gap-3 p-1 pl-1 pr-2 rounded-full hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors">
<img alt="Alex Johnson" class="w-8 h-8 rounded-full border border-slate-200 dark:border-slate-700 object-cover" src="${user.avatarUrl?user.avatarUrl:'https://lh3.googleusercontent.com/aida-public/AB6AXuABry7N6i1FTtV_MXYPdkZekAdwqPcnGtemZV5HamrPSwYMFj8mEFyi4ykrMhtAFpOGeHkdTrnzQv2m6FlHZdpbDd139_teY5pYrLuyx8QMrJhtiWt2tlGuBplBH2qszLN_TyVezmrzx5vDwO4Fc7NmpCGSvJtwBHcSx8gAgaaJLBwpu7Cv2v2Pji4Ec9hPTGMpGlY_iBIsX6PcgYSyV17gCZ9Po4-EvTTT2pPKkEwQMOkYrLLTjKwOs2qeD3xPPLQyyF3_FueCxUJI'}">
<span class="hidden lg:block text-sm font-medium text-slate-700 dark:text-slate-200">${user.email}</span>
<span class="material-symbols-outlined text-slate-400 text-lg">expand_more</span>
</button>
<div class="absolute right-0 mt-2 w-48 bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 rounded-xl shadow-xl opacity-0 invisible group-hover:opacity-100 group-hover:visible transition-all duration-200 py-2">
${user.permissions.includes("USER_UPDATE") ? `<a href="/AdminView.html" class="flex items-center gap-3 px-4 py-2 text-sm text-slate-600 dark:text-slate-300 hover:bg-slate-50 dark:hover:bg-slate-800 hover:text-primary transition-colors" href="#">
<span class="material-icons-outlined text-lg">manage_accounts</span>
                            Account Management
                        </a>`:''}

                        <a href="/ChangePassword.html" class="flex items-center gap-3 px-4 py-2 text-sm text-slate-600 dark:text-slate-300 hover:bg-slate-50 dark:hover:bg-slate-800 hover:text-primary transition-colors">
<span class="material-icons-outlined text-lg">manage_accounts</span>
                            Change Password
                        </a>
<a class="flex items-center gap-3 px-4 py-2 text-sm text-slate-600 dark:text-slate-300 hover:bg-slate-50 dark:hover:bg-slate-800 hover:text-primary transition-colors" href="#">
<span class="material-icons-outlined text-lg">shopping_bag</span>
                            Orders
                        </a>
<div class="h-px bg-slate-100 dark:bg-slate-800 my-1"></div>
<a id = "btnLogOut" class="flex items-center gap-3 px-4 py-2 text-sm text-red-500 hover:bg-red-50 dark:hover:bg-red-900/10 transition-colors" href="#">
<span class="material-icons-outlined text-lg">logout</span>
                            Logout
                        </a>
</div>
</div>`
  document.getElementById("clsLogin").insertAdjacentHTML('afterbegin',layout);
  const btnLogOut = document.getElementById('btnLogOut');
    btnLogOut.addEventListener('click', async () => {
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
    });


}
function renderProduct(products){
    const divProductContent = document.getElementById('divProductContent');
    divProductContent.innerHTML="";
products.forEach(product => {
   const row= `<div class="group bg-white dark:bg-slate-800 rounded-xl overflow-hidden border border-slate-100 dark:border-slate-700 hover:shadow-2xl transition-all">
<div class="relative aspect-square overflow-hidden bg-slate-50 dark:bg-slate-900 p-8">
<img  class="w-full h-full object-contain group-hover:scale-110 transition-transform duration-500"  src="${product.imageUrl}">
<button class="absolute top-4 right-4 p-2 rounded-full bg-white/80 dark:bg-slate-800/80 backdrop-blur-md opacity-0 group-hover:opacity-100 transition-opacity">
<span class="material-icons-outlined text-sm">favorite_border</span>
</button>
</div>
<div class="p-6">
<div class="flex items-center gap-2 mb-3">
<span class="px-2 py-0.5 rounded bg-blue-50 dark:bg-blue-900/30 text-[10px] font-bold text-primary uppercase">M3 Chip</span>
<span class="px-2 py-0.5 rounded bg-slate-50 dark:bg-slate-700 text-[10px] font-bold text-slate-500 uppercase">16GB RAM</span>
</div>
<h3 class="font-bold text-lg mb-1 group-hover:text-primary transition-colors">${product.name}</h3>
<p class="text-sm text-slate-500 mb-4">Ultimate power for pros.</p>
<div class="flex items-center justify-between">
<span class="text-xl font-bold">${formatPrice(product.price)}</span>
<button data-id=${product.id} class="w-10 h-10 rounded-lg bg-primary text-white flex items-center justify-center hover:bg-primary/90 transition-colors btnAddCart">
<span class="material-icons-outlined">add_shopping_cart</span>
</button>
</div>
</div>
</div>`
    divProductContent.innerHTML+=row;
}); 

}

function formatPrice(price){
  const result=  new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD'
}).format(price);
return result;
}
