var app = new Vue({
    el: '#root',
    data: {
        Download: '0 KB/s',
        Upload: '0 KB/s',
        MaxSpeed: '0 KB/s',
        Daydownload: '0 MB',
        Dayupload: '0 MB',
        Monthdownload: '0 GB',
        Monthupload: '0 GB',
        Totaldownload: '0 GB',
        Totalupload: '0 GB',
    },
    mounted() {
        this.fetchData();
        setInterval(() => this.fetchData(), 2000);
    },
    methods: {
        fetchData() {
            axios.get('../Download').then(response => this.Download = response.data);
            axios.get('../Upload').then(response => this.Upload = response.data);
            axios.get('../MaxSpeed').then(response => this.MaxSpeed = response.data);
            axios.get('../Daydownload').then(response => this.Daydownload = response.data);
            axios.get('../Dayupload').then(response => this.Dayupload = response.data);
            axios.get('../Monthdownload').then(response => this.Monthdownload = response.data);
            axios.get('../Monthupload').then(response => this.Monthupload = response.data);
            axios.get('../Totaldownload').then(response => this.Totaldownload = response.data);
            axios.get('../Totalupload').then(response => this.Totalupload = response.data);
        },
        ClickDownload() {
            axios.get('../Download').then(response => this.Download = response.data);
        },
        ClickUpdate() {
            axios.get('../Upload').then(response => this.Upload = response.data);
        }
    }
});
Vue.config.devtools = true;