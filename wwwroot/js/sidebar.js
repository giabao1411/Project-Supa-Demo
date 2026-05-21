// /src/components/sidebar.js

class AppSidebar extends HTMLElement {
    connectedCallback() {
        this.render();
        this.bindEvents();
        this.setActive();
    }

    render() {
        this.innerHTML = `
             <!-- Sidebar Navigation -->
    <aside class="w-64 border-r border-slate-200 dark:border-slate-800 bg-white dark:bg-background-dark flex flex-col fixed h-full z-30">
      <div class="p-6 flex items-center gap-3">
    <div class="w-8 h-8 bg-primary rounded flex items-center justify-center">
      <span class="material-icons text-white text-lg">memory</span>
    </div>
    <span class="text-xl font-bold tracking-tight text-slate-900 dark:text-white leading-none">TechStore<br/><span class="text-xs font-normal text-slate-500 uppercase tracking-widest">Admin Panel</span></span>
  </div>
  <nav class="flex-1 px-4 mt-4 space-y-1">
    <a data-link class="flex items-center px-4 py-3 text-slate-600 dark:text-slate-400 hover:bg-slate-100 dark:hover:bg-slate-800 rounded-lg transition-colors group" href="#">
      <span class="material-icons mr-3">dashboard</span>
<span class="font-medium">Dashboard</span>
</a>
<a data-link class="flex items-center px-4 py-3 text-primary bg-primary/10 rounded-lg transition-colors group" href="#">
  <span class="material-icons mr-3">people</span>
<span class="font-medium">Users</span>
</a>
<a data-link class="flex items-center px-4 py-3 text-slate-600 dark:text-slate-400 hover:bg-slate-100 dark:hover:bg-slate-800 rounded-lg transition-colors group" href="./Product.html">
  <span class="material-icons mr-3">inventory_2</span>
  <span class="font-medium">Products</span>
</a>
<a data-link class="flex items-center px-4 py-3 text-slate-600 dark:text-slate-400 hover:bg-slate-100 dark:hover:bg-slate-800 rounded-lg transition-colors group" href="#">
  <span class="material-icons mr-3">shopping_cart</span>
  <span class="font-medium">Orders</span>
</a>
<a data-link href="./Setting.html" class="flex items-center px-4 py-3 text-slate-600 dark:text-slate-400 hover:bg-slate-100 dark:hover:bg-slate-800 rounded-lg transition-colors group" href="#">
<span class="material-icons mr-3">settings</span>
<span class="font-medium">Settings</span>
</a>
</nav>
            <div class="p-4 mt-auto border-t border-slate-100 dark:border-slate-800">
        <div class="flex items-center p-2 rounded-lg bg-slate-50 dark:bg-slate-800/50">
    <img class="w-10 h-10 rounded-full object-cover" data-alt="Admin user profile picture" src="https://lh3.googleusercontent.com/aida-public/AB6AXuC5c_THALLACnxt1KbYOD9UTv9xXZRVs7zv9jWKIp27XDqMFuEZ3pyd9WskKA35BqUm_PorbOGmTyoiAH-7uXqf2B5WyBoTGM1ui7BnwyzF6xb5HptQulAdK9hnWMAA3zaH209KU57qrTwwgIp33RdLyndFKwrOvuBHupeIc8-kgKvm6GSMs5d_8H-jKWqlcxfwgkPCFhXfGU-PS-WXJ-z-JGtX0UY3ja88FWj-Qu8l3ERYWBq0l53rvxLB2ZzTMvUbcRmCOgJ1N1FA"/>
    <div class="ml-3">
<p class="text-sm font-semibold text-slate-900 dark:text-white">Alex Morgan</p>
<p class="text-xs text-slate-500">Super Admin</p>
            </div>
            <button class="ml-auto text-slate-400 hover:text-primary">
            <span class="material-icons text-xl">logout</span>
            </button>
        </div>
    </div>
</aside>
        `;
    }

    bindEvents() {
        this.querySelectorAll("[data-link]").forEach(link => {
            link.addEventListener("click", (e) => {
                e.preventDefault();

                const url = link.getAttribute("href");

                history.pushState(null, null, url);

                // trigger router
                window.dispatchEvent(new Event("route-change"));
            });
        });
    }

    setActive() {
        const path = window.location.pathname;

        this.querySelectorAll("[data-link]").forEach(link => {
            link.classList.remove("active");

            if (link.getAttribute("href") === path) {
                link.classList.add("active");
            }
        });
    }
}

customElements.define("app-sidebar", AppSidebar);