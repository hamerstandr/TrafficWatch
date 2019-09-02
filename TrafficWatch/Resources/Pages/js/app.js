var app = new Vue({
    el: '#root',
    data: {
        Messages: Mes,
        Download: '0kb',
        Upload: '0kb',
        MaxSpeed: '0kb',
        Daydownload: '0kb',
        Dayupload: '0kb',
        Monthdownload: '0kb',
        Monthupload: '0kb',
        Totaldownload: '0kb',
        Totalupload: '0kb',
    },
    mounted() {
        UploadUpdate();
        DownloadUpdate();
        axios.get('../MaxSpeed').then(response => this.MaxSpeed = response.data);
        axios.get('../Daydownload').then(response => this.Daydownload = response.data);
        axios.get('../Dayupload').then(response => this.Dayupload = response.data);
        axios.get('../Monthdownload').then(response => this.Monthdownload = response.data);
        axios.get('../Monthupload').then(response => this.Monthupload = response.data);
        axios.get('../Totaldownload').then(response => this.Totaldownload = response.data);
        axios.get('../Totalupload').then(response => this.Totalupload = response.data);
        // using a Update to run the Update() function every second
        function UploadUpdate() {
            axios.get('../Upload').then(response => callUpload(response));
        }
        var indexUpload = 10000;
        function callUpload(response) {
            app.Upload = response.data;
            console.log(app.Upload);
            //indexUpload += 100;
            //if (indexUpload > 50000)
            //    indexUpload = 10000;
            //setInterval(UploadUpdate, indexUpload);
        }
        function DownloadUpdate() {
            axios.get('../Download').then(response => callDownload(response));

        }
        var indexDownload = 20000;
        function callDownload(response) {
            app.Download = response.data;
            console.log(app.Download);
            //indexDownload += 100;
            //if (indexDownload > 80000)
            //    indexDownload = 20000;
            //setInterval(DownloadUpdate, indexDownload);
        }

        // // GET request for remote image
        // axios({
        //     method: 'get',
        //     url: '../MaxSpeed',
        //     responseType: 'stream'
        // })
        //     .then(function (response) {
        //         console.log(response);
        //         alert(response.data);
        //         this.MaxSpeed = response.data;
        //     });

    }
    ,
    methods: {
        messageshow(name) {
            Mes.forEach(element => {
                if (name == element.name) {
                    element.isVisible = true;
                }
            });
        }
        ,
        ClickDownload() {
            axios.get('../Download').then(response => app.Download = response.data);
        },
        ClickUpdate() {
            axios.get('../Upload').then(response => app.Upload = response.data);
        }
    }
});
Vue.config.devtools = true;