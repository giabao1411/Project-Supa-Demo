export class Pagination {

    constructor({ containerId, infoId, pageSize = 10, onPageChange }) {

        this.page = 1;
        this.pageSize = pageSize;
        this.totalItems = 0;
        this.totalPages = 0;

        this.container = document.getElementById(containerId);
        this.info = document.getElementById(infoId);

        this.onPageChange = onPageChange;
    }

    update(totalItems) {

        this.totalItems = totalItems;
        this.totalPages = Math.ceil(totalItems / this.pageSize);

        this.renderInfo();
        this.renderPagination();
    }

    changePage(page) {

        if (page < 1 || page > this.totalPages) return;

        this.page = page;

        this.onPageChange(page);
    }

    renderInfo() {
        this.info.innerHTML="";
        const start = (this.page - 1) * this.pageSize + 1;
        let end = this.page * this.pageSize;

        if (end > this.totalItems) end = this.totalItems;

        this.info.innerHTML =
        `
        Showing <span class="text-slate-900 dark:text-white">${start}</span> to <span class="text-slate-900 dark:text-white">${end}</span> of <span class="text-slate-900 dark:text-white">${this.totalItems}</span> results
        `
            ;
    }

    renderPagination() {

        this.container.innerHTML = "";

        // previous
        this.container.innerHTML += `
            <button 
                class="px-3 py-2 border rounded-l-md bg-white text-gray-500 hover:bg-gray-100"
                ${this.page === 1 ? "disabled" : ""}
                data-page="${this.page - 1}">
                Previous
            </button>
        `;

        for (let i = 1; i <= this.totalPages; i++) {

            if (
                i === 1 ||
                i === this.totalPages ||
                (i >= this.page - 1 && i <= this.page + 1)
            ) {

                this.container.innerHTML += `
                    <button 
                        class="px-3 py-2 border 
                        ${i === this.page
                        ? "bg-blue-600 text-white"
                        : "bg-white text-gray-500 hover:bg-gray-100"}"
                        data-page="${i}">
                        ${i}
                    </button>
                `;

            } else if (
                i === this.page - 2 ||
                i === this.page + 2
            ) {

                this.container.innerHTML += `
                    <span class="px-3 py-2 border bg-white text-gray-500">
                        ...
                    </span>
                `;
            }
        }

        // next
        this.container.innerHTML += `
            <button 
                class="px-3 py-2 border rounded-r-md bg-white text-gray-500 hover:bg-gray-100"
                ${this.page === this.totalPages ? "disabled" : ""}
                data-page="${this.page + 1}">
                Next
            </button>
        `;

        this.bindEvents();
    }

    bindEvents() {

        const buttons = this.container.querySelectorAll("button[data-page]");

        buttons.forEach(btn => {

            btn.addEventListener("click", () => {

                const page = parseInt(btn.dataset.page);

                this.changePage(page);
            });

        });
    }
}