

import { apiFetch } from "./JsCommon.js";
class ChoicesHelper {

    static instances = {};

    static bind(selector, options = {}) {

        const element = document.querySelector(selector);

        const instance = new Choices(element, {
            removeItemButton: true,
            searchEnabled: true,
            shouldSort: false,
          
            ...options
        });

        this.instances[selector] = instance;

        return instance;
    }

    static async bindApi(selector, apiUrl, valueField = "id", labelField = "name") {

        const element = document.querySelector(selector);

        // destroy instance cũ nếu tồn tại
        // if (this.instances[selector]) {
        //     this.instances[selector].destroy();
        // }
        if(this.instances[selector]){
            return this.instances[selector];
        }

        const instance = new Choices(element, {
            removeItemButton: true,
            searchEnabled: true,
            shouldSort: false
        });
        

        const res = await apiFetch(apiUrl);
        const data = await res.json();

        const choices = data.map(item => ({
            value: item[valueField],
            label: item[labelField]
        }));

        instance.setChoices(choices, 'value', 'label', true);

        this.instances[selector] = instance;

        return instance;
    }

    static setValue(selector, values) {

        const instance = this.instances[selector];

        if (!instance) return;

        instance.setChoiceByValue(values);
    }

    static getValue(selector) {

        const instance = this.instances[selector];

        if (!instance) return [];

        return instance.getValue(true);
    }

    static clear(selector) {

        const instance = this.instances[selector];

        if(!instance) return;

        instance.removeActiveItems();
    }

}

export default ChoicesHelper;