import CONFIG from "./config.js";
import { apiFetch } from "./JsCommon.js";
const APICart = CONFIG.API_CART;
const APIOrder = CONFIG.API_ORDER;
const API = CONFIG.API_AUTH;
const modalCart = new Modal(document.getElementById('divCart'));
const timers = {};
document.getElementById('btnShowCart').addEventListener("click",function(){
    loadCart();
    modalCart.show();
})
document.getElementById('btnCloseCart').addEventListener("click",function(){
    modalCart.hide();
})
async function loadCart(){
    const divCartItems = document.getElementById('divCartItems');
    divCartItems.innerHTML="";
    const res = await apiFetch(`${APICart}`,{
        method: "GET",
    });
    
    const data = await res.json();
    if (!data.items || data.items.length === 0){
        document.getElementById("subTotalCart").innerText=formatPrice(0);
        document.getElementById("totalCart").innerText=formatPrice(0);
        return;
    }
    document.getElementById("lblTotalCartItem").innerText=data.items.length;
    data.items.forEach(item =>{
        const row =`<div class="flex gap-6 group">
<div class="w-24 h-24 rounded-lg bg-slate-50 overflow-hidden flex-shrink-0 border border-slate-100">
<img alt="MacBook Pro" class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500" src=${item.imageUrl}>
</div>
<div class="flex-1 flex flex-col justify-between py-1">
<div class="flex justify-between items-start">
<div>
<h3 class="font-bold text-slate-900">${item.productName}</h3>
<p class="text-xs text-slate-500 mt-1">14-inch, 32GB RAM, 1TB</p>
</div>
<span data-id=${item.id} class="material-symbols-outlined text-slate-400 hover:text-red-500 cursor-pointer transition-colors text-xl deleteCartItem">delete</span>
</div>
<div class="flex justify-between items-center mt-4">
<div class="flex items-center border border-slate-200 rounded-lg overflow-hidden">
<button data-productid="${item.productId}" class="px-2 py-1 hover:bg-slate-50 text-slate-600 transition-colors text-xs font-bold btnMinusQuantity ">-</button>
<span id="qty-${item.productId}" class="px-3 py-1 text-xs font-bold text-slate-900">${item.quantity}</span>
<button data-productid="${item.productId}" class="px-2 py-1 hover:bg-slate-50 text-slate-600 transition-colors text-xs font-bold btnPlusQuantity">+</button>
</div>
<span id="totalProducts-${item.productId}" class="font-bold text-primary text-lg">${formatPrice(item.total)}</span>
</div>
</div>
</div>`;
        divCartItems.innerHTML+=row;
    });
        document.getElementById("subTotalCart").innerText=formatPrice(data.total);
        document.getElementById("totalCart").innerText=formatPrice(data.total);

}
document.getElementById("btnCheckOut").addEventListener("click",function(e){
    e.preventDefault();
    checkout();
})
async function isLogin(){
    const res =  await apiFetch(`${API}/me`);
    if(res.ok){
        return true;
    }
    return false;
    
}

document.addEventListener("click",async function (e) {
    const btnAddCart = e.target.closest('.btnAddCart');
    const btnDeleteCartItem= e.target.closest('.deleteCartItem');
    const btnPlusQuantity = e.target.closest('.btnPlusQuantity');
    const btnMinusQuantity = e.target.closest('.btnMinusQuantity');
    if(btnAddCart){
        const checkLogin =  await isLogin();
        console.log(checkLogin);
        if(!checkLogin){
            alert("Please login to add product!");
            return;
        }
        const productId = btnAddCart.dataset.id;
        await addToCart(productId);
    }
    if(btnDeleteCartItem){
        const cartItem = btnDeleteCartItem.dataset.id;
        await removeItem(cartItem);

    }
    if(btnPlusQuantity){
        const productId = btnPlusQuantity.dataset.productid;
        increase(productId);
    }
    if(btnMinusQuantity){
        const productId = btnMinusQuantity.dataset.productid;
        decrease(productId);
    }
});
  
function increase(productId) {
    const input = document.getElementById(`qty-${productId}`);
    let value = parseInt(input.innerText) || 1;
    value++;

    input.innerText = value;
    changeQuantity(productId, value);
}

function decrease(productId) {
    const input = document.getElementById(`qty-${productId}`);
    let value = parseInt(input.innerText) || 1;

    if (value > 1) value--;

    input.innerText = value;
    changeQuantity(productId, value);
}


function changeQuantity(productId, quantity) {
    // updateUI(productId, quantity);

    if (timers[productId]) {
        clearTimeout(timers[productId]);
    }

    timers[productId] = setTimeout(async () => {
       const data= await callUpdateAPI(productId, quantity);
        applyPartialUpdate(data);
        // 🔥 gọi lại load cart sau khi update
        // await loadCart();

    }, 1000);
}
async function callUpdateAPI(productId, quantity) {
  const res=  await apiFetch(`${APICart}/update-quantity`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        credentials: "include",
        body: JSON.stringify({
            productId,
            quantity
        })
    });
    return await res.json();
}
function applyPartialUpdate(data) {
    const { productId, quantity, itemTotal, cartTotal } = data;

    // update quantity (đề phòng lệch)
    const input = document.getElementById(`qty-${productId}`);
    if (input) input.innerText = quantity;

    // update item total
    const totalEl = document.getElementById(`totalProducts-${productId}`);
    if (totalEl) {
        totalEl.innerText = formatPrice(itemTotal);
    }

    // update cart total
  const totalSub=  document.getElementById("subTotalCart");
   const totalCart=     document.getElementById("totalCart");
    if (totalSub) {
        totalSub.innerText = formatPrice(cartTotal);
    }
    if(totalCart){
        totalCart.innerText=formatPrice(cartTotal);
    }
}
async function addToCart(productId) {
    try {
        const res = await apiFetch(`${APICart}/add`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                productId: productId,
                quantity: 1
            })
        });

        if (!res.ok) {
            const err = await res.text();
            alert(err);
            return;
        }

        alert("Đã thêm vào giỏ hàng");
        await loadCart();
        

    } catch (err) {
        console.error(err);
    }
    
}
async function removeItem(itemId) {
    if (!confirm("Bạn có chắc muốn xóa?")) return;

    try {
        const res = await apiFetch(`${APICart}/remove/${itemId}`, {
            method: "DELETE"
        });

        if (!res.ok) {
            alert("Xóa thất bại");
            return;
        }

        loadCart();

    } catch (err) {
        console.error(err);
    }
}
async function checkout() {
    try {
        const res = await apiFetch(`${APIOrder}/checkout`, {
            method: "POST",
            credentials: "include"
        });

        const data = await res.json();

        if (!res.ok) {
            throw new Error(data.message);
        }

        alert("Đặt hàng thành công!");
        modalCart.hide();
        loadCart(); // reload giỏ hàng

    } catch (err) {
        console.error(err);
        alert("Checkout thất bại");
    }
}
function formatPrice(price) {
        const result = new Intl.NumberFormat('en-US', {
            style: 'currency',
            currency: 'USD'
        }).format(price);
        return result;
    }