import { apiFetch, LogOut } from "./JsCommon.js";
import { Pagination } from "./pagination.js";
import ChoicesHelper from "./choicesHelper.js";
import CONFIG from "./config.js";
const APIAdmin = CONFIG.API_PRODUCT;
const APIAuth = CONFIG.API_AUTH;

const modal = new Modal(document.getElementById('deleteModal'));
const modalEdit = new Modal(document.getElementById('addProductModal'), {
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
let currentPage = 1;

let editingId = null;
// await ChoicesHelper.bindApi("#dropDownRoles",`${APIAdmin}/list-roles`);
const pagination = new Pagination({
    containerId: "divPagination",
    infoId: "divResultInfo",
    pageSize: pageSize,
    onPageChange: handlePageChange
});
// Event listeners
document.getElementById("product-image").addEventListener("click", function () {
    this.value = null;
});

document.getElementById("product-image").addEventListener("change", function () {
    const preview = document.getElementById("preview");
    const input = document.getElementById("product-image");
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
        };

        preview.appendChild(div);
    }
});
async function uploadImages(files) {
    const formData = new FormData();

    for (let file of files) {
        formData.append("files", file);
    }

    const res = await apiFetch(`${APIAdmin}/upload`, {
        method: "POST",
        body: formData
    });

    if (!res.ok) throw new Error("Upload failed");

    return await res.json(); // trả về list url
}
async function initEvents() {
    document.getElementById("btnLogOut").addEventListener("click", function (e) {
        e.preventDefault();
        LogOut();
    })

    document.getElementById("btnAddProduct").addEventListener("click", function (e) {
        e.preventDefault();
        bindTitlePopupProduct("Add New Product","Enter product details to add to your inventory.","Create Product");
        modalEdit.show();
    });
    document.getElementById("btnCloseProductModal").addEventListener("click", function (e) {
        e.preventDefault();
        modalEdit.hide();
    });
    document.getElementById("btnCancelProductModal").addEventListener("click", function (e) {
        e.preventDefault();
        modalEdit.hide();
    });
    document.getElementById("txtSearch").addEventListener("input", function (e) {
        e.preventDefault();
        var categoryId = document.getElementById("ddlSearch-Category").value;
        keyword = e.target.value.trim();
        clearTimeout(debounceTimer);
        debounceTimer = setTimeout(() => {
            searchProduct(e.target.value.trim(), categoryId);
        }, 400);

    });
    document.getElementById("btnCancelDelete").addEventListener("click", async function (e) {
        modal.hide();
        id = null;
    });
    document.getElementById("btnConfirmDelete").addEventListener("click", async function (e) {

        await deleteUser(id);
        modal.hide();
        id = null;
    });

    document.addEventListener('click', async function (event) {
        const btnDelete = event.target.closest(".btnDeleteProduct");
        const btnEdit = event.target.closest(".btnEditProduct");
        if (btnDelete) {
            id = btnDelete.dataset.id;

            modal.show()

        } if (btnEdit) {
            id = btnEdit.dataset.id;
            await bindEditUser(id);
            modalEdit.show();
        }
    });

    const btnCreateUser = document.getElementById("btnCreateProduct");
    btnCreateUser.addEventListener("click", async function (e) {
        e.preventDefault();

        const name = document.getElementById("product-name").value;
        const price = +document.getElementById("product-price").value;
        const stock = +document.getElementById("product-stock").value;
        // const files = document.getElementById("product-image").files;
        const categoryId = document.getElementById("product-category").value;
        let imageUrls = [];
        // console.log(files);
        if (files.length > 0) {
            imageUrls = await uploadImages(files);
           
        }
        if (!isEdit) {
            await CreateProduct(name, price, stock, imageUrls, categoryId);
        } else {
             filesEdit.push(...imageUrls);
            await UpdateProduct(id, name, price, stock, categoryId, filesEdit);
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
        document.getElementById("lblUserName").innerText = user.email;
        document.getElementById("lblRole").innerText = user.roles[0].name;
        // document.getElementById("contentAdminView").style.display = "block";
        LoadData();
    }
}
//
// Admin functions
async function CreateProduct(name, price, stock, imageUrls, categoryId) {
   
    const res = await apiFetch(`${APIAdmin}/create`, {
        method: 'POST',
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            name: name,
            price: price,
            stock: stock,
            images: imageUrls,
            categoryId: categoryId
        }),
    });
    if (res.ok) {
        alert("Product created successfully");
        LoadData();
        modalEdit.hide();
    }
    else {
        alert("Failed to create product");
        LoadData();
        modalEdit.hide();
    }
}
async function UpdateProduct(id, name, price, stock, categoryId, images) {
    // const rolesIs=dropDownRoles.map(id =>({
    //                 id: id
    //             }));
    const resEdit = await apiFetch(`${APIAdmin}/${id}`, {
        method: "PUT",
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            name: name,
            price: price,
            stock: stock,
            categoryId: categoryId,
            images: images
        })
    });
    if (resEdit.ok) {
        alert("Product updated successfully");
        LoadData();
        modalEdit.hide();
    } else {
        alert("Failed to update Product");
        LoadData();
        modalEdit.hide();
    }

}
async function deleteUser(id) {
    const res = await apiFetch(`${APIAdmin}/${id}`, {
        method: "DELETE",
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },


    });
    if (res.ok) {
        alert("Product deleted successfully");
        LoadData();
    } else {
        alert("Failed to delete Product");
        LoadData();
    }
}
async function searchProduct(searchValue, categoryId, page = 1) {
    if (controller) {
        controller.abort();
    }

    controller = new AbortController();
    keyword = document.getElementById("txtSearch").value.trim();
    isSearching = true;
    const tableBody = document.getElementById('productTable');
    tableBody.innerHTML = '';
    if (searchValue === "") {
        keyword = "";
        isSearching = false;
        pagination.page = 1;
        LoadData();
        return;
    }
    const res = await apiFetch(`${APIAdmin}/products-by-keyword`, {
        method: "POST",
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            keyword: searchValue,
            categoryId: categoryId,
            page: page,
            pageSize: pageSize
        })
    });
    if (res.ok) {
        const products = await res.json();
        bindTable(products.items, tableBody);
        pagination.update(products.totalItems);
    } else {
        tableBody.innerHTML = '<tr><td colspan="5" class="px-6 py-4 text-center text-slate-500">No users found.</td></tr>';
    }
}
async function handlePageChange(page) {

    if (isSearching) {
        var categoryId = document.getElementById("ddlSearch-Category").value;
        await searchProduct(keyword, categoryId, page);

    } else {

        await LoadData(page);

    }
}
//
// Helper functions
function clearEditModal() {
    document.getElementById('product-name').value = '';
    document.getElementById('product-price').value = '';
    document.getElementById('product-stock').value = '';
    document.getElementById('product-category').value = '';
    document.getElementById('preview').innerHTML = '';
    document.getElementById('product-image').value = '';
    isEdit = false;
    id = null;
    files = [];
    filesEdit=[];
    // ChoicesHelper.clear("#dropDownRoles");
}
function formatDate(dateString) {
    const date = new Date(dateString);
    const options = { month: 'short', day: 'numeric', year: 'numeric' };
    return date.toLocaleDateString("en-US", options);
}
//
// Data loading functions
async function LoadData(page = 1) {
    // document.getElementById('idUserTarget').value = "";
    const tableBody = document.getElementById('productTable');
    tableBody.innerHTML = '';
    const res = await apiFetch(`${APIAdmin}/products?page=${page}&pageSize=${pageSize}`, {
        method: 'GET',
        credentials: 'include',
        header: { 'Content-Type': 'application/json' }
    });
    if (res.ok) {
        const products = await res.json();
        bindTable(products.items, tableBody);
        pagination.update(products.totalItems);
    } else {
        window.location.href = '/Error.html';
        // console.log("loi loaddata")
    }
    // await getRoles('Select role', document.getElementById("dropDownRoles"));
    // await getRoles('All Roles', document.getElementById("ddlRoles"), false);


}
async function bindTable(products, tableBody) {
    if (products.length === 0) {
        tableBody.innerHTML = '<tr><td colspan="5" class="px-6 py-4 text-center text-slate-500">No products found.</td></tr>';
        return;
    }

    products.forEach(product => {
        const row = `<tr class="hover:bg-slate-50 dark:hover:bg-slate-700/30 transition-colors">
<td class="px-6 py-4">
<input class="rounded border-slate-300 dark:border-slate-600 text-primary focus:ring-primary" type="checkbox"/>
</td>
<td class="px-6 py-4">
<div class="flex items-center gap-3">
<div class="h-10 w-10 rounded bg-slate-100 dark:bg-slate-700 flex-shrink-0 bg-cover bg-center"  style="background-image: url('${product.imageUrl}')"></div>
<div>
<p class="text-sm font-semibold">${product.name}</p>

</div>
</div>
</td>

<td class="px-6 py-4 text-sm font-medium">$${product.price}</td>
<td class="px-6 py-4">
<span class="inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-xs font-bold bg-${product.stock > 10 ? "emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400emerald" : "orange-100 text-orange-700 dark:bg-orange-900/30 dark:text-orange-400"}">
<span class="h-1.5 w-1.5 rounded-full bg-${product.stock > 10 ? "emerald-600" : "orange-600"}"></span>
                                        ${product.stock > 10 ? "In stock" : "Low Stock"}
                                    </span>
</td>
<td class="px-6 py-4 text-right">
<div class="flex justify-end gap-2 text-slate-400">
<button data-id="${product.id}" class="hover:text-primary transition-colors btnEditProduct"><span class="material-symbols-outlined text-lg ">edit</span></button>
<button data-id="${product.id}" class="hover:text-red-500 transition-colors btnDeleteProduct"><span class="material-symbols-outlined text-lg ">delete</span></button>
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
function bindTitlePopupProduct(titlePopup, description, nameBtn) {
    document.getElementById("titlePopupProduct").innerText = titlePopup;
    document.getElementById("descriptionPopupProduct").innerText = description;
    document.getElementById("btnCreateProduct").innerText = nameBtn;
}
async function bindEditUser(id) {
     bindTitlePopupProduct("Update Product","Enter product details to update to your inventory.","Update Product");
    // document.getElementById("idUserTarget").value = userId;
    isEdit = true;


    const res = await apiFetch(`${APIAdmin}/${id}`, {
        method: "GET",
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
    }
    );
    const data = await res.json();




    document.getElementById("product-name").value = data.name;
    document.getElementById("product-price").value = data.price;
    document.getElementById("product-stock").value = data.stock;
    document.getElementById("product-category").value = data.categoryId;
    const preview = document.getElementById("preview");
    preview.innerHTML = "";

    for (let file of data.images) {
        filesEdit.push(file);
        const div = document.createElement("div");
        div.className = "relative group aspect-[4/3]";

        div.innerHTML = `
        <img src="${file}" class="w-full h-full object-contain"/>

        <button class="absolute top-1 right-1 bg-black/60 text-white rounded-full w-6 h-6 text-xs opacity-0 group-hover:opacity-100 transition">
          ✕
        </button>

        <div class="absolute bottom-0 left-0 w-full h-1 bg-gray-200 rounded">
          <div class="progress h-full bg-blue-500 w-0 rounded"></div>
        </div>
      `;
        // remove
        div.querySelector("button").onclick = () => {
            filesEdit = filesEdit.filter(f => f !== file);
            div.remove();
        };

        preview.appendChild(div);

    }
    console.log(files);

};
//
