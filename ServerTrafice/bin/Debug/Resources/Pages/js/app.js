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
        axios.get('../Upload').then(response => this.Upload = response.data);
        axios.get('../Download').then(response => this.Download = response.data);
        axios.get('../MaxSpeed').then(response => this.MaxSpeed = response.data);
        axios.get('../Daydownload').then(response => this.Daydownload = response.data);
        axios.get('../Dayupload').then(response => this.Dayupload = response.data);
        axios.get('../Monthdownload').then(response => this.Monthdownload = response.data);
        axios.get('../Monthupload').then(response => this.Monthupload = response.data);
        axios.get('../Totaldownload').then(response => this.Totaldownload = response.data);
        axios.get('../Totalupload').then(response => this.Totalupload = response.data);
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
        }//,
        //MaxSpeedShow() {
        //    $.ajax({
        //        type: 'GET',
        //        url: '..\MaxSpeed',
        //        success: function (html) {
        //            //alert(html);
        //            MaxSpeed = html;
        //        }
        //    });
    }
});
Vue.config.devtools = true;