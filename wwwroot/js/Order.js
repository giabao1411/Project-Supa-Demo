import { apiFetch } from "./JsCommon.js";
import { Pagination } from "./pagination.js";
import CONFIG from "./config.js";
const APIOder = CONFIG.API_ORDER;
const APIAuth = CONFIG.API_AUTH;
let pageSize=10;
let isSearching;
let keyWord="";
let controller;
let debounceTimer;
const pagination = new Pagination({
    containerId: "divPagination",
    infoId: "divResultInfo",
    pageSize: pageSize,
    onPageChange: handlePageChange
});
initAdminCheck();
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
        // document.getElementById("contentAdminView").style.display = "block";
        LoadOrder();
    }
}
async function LoadOrder(page=1,keyWord=""){
    const tblOrdersBody = document.getElementById("tblOrdersBody");
    console.log(document.getElementById("txtSearch").value);
    tblOrdersBody.innerHTML="";
    const res = await apiFetch(`${APIOder}/get?page=${page}&pageSize=${pageSize}&KeyWord=${keyWord}`);
    if(res.ok){
        const data = await res.json();
        BindOrders(data.items,tblOrdersBody);
        pagination.update(data.totalItems)
    }
}
 document.getElementById("txtSearch").addEventListener("input", function (e) {
        e.preventDefault();
        
        keyWord = e.target.value.trim();
        clearTimeout(debounceTimer);
        debounceTimer = setTimeout(() => {
            searchOrders(e.target.value.trim());
        }, 400);

    });
    async function searchOrders(searchValue,page=1) {
        if (controller) {
            controller.abort();
        }
    
        controller = new AbortController();
        keyWord = document.getElementById("txtSearch").value.trim();
        isSearching = true;
        const tableBody = document.getElementById('tblOrdersBody');
        tableBody.innerHTML = '';
        if (searchValue === "") {
            keyWord = "";
            isSearching = false;
            pagination.page = 1;
            LoadOrder();
            return;
        }
        else
        {
            LoadOrder(page,keyWord)
        }
        
    }
async function handlePageChange(page) {

    if (isSearching) {
        // var categoryId = document.getElementById("ddlSearch-Category").value;
        await searchOrders(keyWord, page);

    } else {

        await LoadOrder(page);

    }
}
function formatDate(dateString) {
    const date = new Date(dateString);
    const options = { month: 'short', day: 'numeric', year: 'numeric' };
    return date.toLocaleDateString("en-US", options);
}
function formatPrice(price) {
        const result = new Intl.NumberFormat('en-US', {
            style: 'currency',
            currency: 'USD'
        }).format(price);
        return result;
    }
async function BindOrders(data,tbOrder){
 if(data.length===0)
 {
    tbOrder.innerHTML = '<tr><td colspan="6" class="px-6 py-4 text-center text-slate-500">No orders found.</td></tr>';
        return;
 }
 data.forEach(order =>{
 const row = ` 
<tr class="hover:bg-slate-50 transition-colors group">
<td class="px-6 py-4">
<input class="rounded border-slate-300 dark:border-slate-600 text-primary focus:ring-primary" type="checkbox"/>
</td>
<td class="px-6 py-4">
<div class="flex items-center gap-3">

<span class="font-medium text-slate-900">${order.customerName}</span>
</div>
</td>
<td class="px-6 py-4 text-slate-600 text-sm">${formatDate(order.date)}</td>
<td class="px-6 py-4 font-bold text-slate-900">${formatPrice(order.amount)}</td>
<td class="px-6 py-4">
<span class="px-3 py-1 bg-green-100 text-green-700 text-[10px] font-bold uppercase tracking-wider rounded-full">${order.status}</span>
</td>
<td class="px-6 py-4 text-right">
<div class="flex justify-end gap-2 opacity-0 group-hover:opacity-100 transition-opacity">
<button class="p-1.5 hover:bg-primary/10 text-slate-400 hover:text-primary transition-all rounded">
<span class="material-symbols-outlined text-xl">visibility</span>
</button>
<button class="p-1.5 hover:bg-primary/10 text-slate-400 hover:text-primary transition-all rounded">
<span class="material-symbols-outlined text-xl">edit</span>
</button>
</div>
</td>
</tr>`;
        tbOrder.innerHTML+=row;
 })
   
}