var Mes = [];
Vue.component('message', {
    props: {
        title: {
            required: true
        },
        body: {
            required: true
        },
        name: {
            required: true
        },
        isvisibled: {
            default: true
        }
    },
    data() {
        return {
            isVisible: true
        }
    },
    mounted() {
        if (this.isvisibled == true)
            this.isVisible = true;
        else
            this.isVisible = false;
        Mes.push(this);
    },
    template: `<article class="message"  v-show="isVisible">
  <div class="message-header">
    <p>{{title}}</p>
    <button class="delete" aria-label="delete" @click="deletemessag"></button>
  </div>
  <div class="message-body">
    {{body}}
  </div>
</article>`,
    methods: {
        deletemessag() {
            this.isVisible = false;
        }

    }
});