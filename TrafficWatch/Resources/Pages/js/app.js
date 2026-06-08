// translations
const translations = {
    fa: {
        titles: {
            trafficMonitor: 'مانیتور ترافیک شبکه',
            realTimeMonitoring: 'نمایش لحظه‌ای ترافیک',
            online: 'آنلاین',
            currentSpeed: 'سرعت فعلی',
            statistics: 'آمار و اطلاعات'
        },
        labels: {
            download: 'دانلود',
            upload: 'آپلود',
            maxSpeed: 'حداکثر سرعت',
            todayDownload: 'دانلود امروز',
            todayUpload: 'آپلود امروز',
            monthDownload: 'دانلود ماه',
            monthUpload: 'آپلود ماه',
            totalDownload: 'کل دانلود',
            totalUpload: 'کل آپلود'
        },
        buttons: {
            refreshDownload: 'بروزرسانی دانلود',
            refreshUpload: 'بروزرسانی آپلود',
            refreshAll: 'بروزرسانی همه'
        },
        install: {
            message: 'برنامه را نصب کنید تا دسترسی سریع‌تری داشته باشید',
            button: 'نصب برنامه'
        }
    },
    en: {
        titles: {
            trafficMonitor: 'Network Traffic Monitor',
            realTimeMonitoring: 'Real-time traffic monitoring',
            online: 'Online',
            currentSpeed: 'Current Speed',
            statistics: 'Statistics'
        },
        labels: {
            download: 'Download',
            upload: 'Upload',
            maxSpeed: 'Max Speed',
            todayDownload: 'Today Download',
            todayUpload: 'Today Upload',
            monthDownload: 'Month Download',
            monthUpload: 'Month Upload',
            totalDownload: 'Total Download',
            totalUpload: 'Total Upload'
        },
        buttons: {
            refreshDownload: 'Refresh Download',
            refreshUpload: 'Refresh Upload',
            refreshAll: 'Refresh All'
        },
        install: {
            message: 'Install the app for quicker access',
            button: 'Install App'
        }
    },
    ar: {
        titles: {
            trafficMonitor: 'مراقب حركة الشبكة',
            realTimeMonitoring: 'مراقبة حركة المرور في الوقت الحقيقي',
            online: 'متصل',
            currentSpeed: 'السرعة الحالية',
            statistics: 'الإحصائيات'
        },
        labels: {
            download: 'تنزيل',
            upload: 'رفع',
            maxSpeed: 'السرعة القصوى',
            todayDownload: 'تنزيل اليوم',
            todayUpload: 'رفع اليوم',
            monthDownload: 'تنزيل الشهر',
            monthUpload: 'رفع الشهر',
            totalDownload: 'إجمالي التنزيل',
            totalUpload: 'إجمالي الرفع'
        },
        buttons: {
            refreshDownload: 'تحديث التنزيل',
            refreshUpload: 'تحديث الرفع',
            refreshAll: 'تحديث الكل'
        },
        install: {
            message: 'قم بتثبيت التطبيق للوصول بشكل أسرع',
            button: 'تثبيت التطبيق'
        }
    }
};

// Get language from URL or default to fa
function getLanguage() {
    const urlParams = new URLSearchParams(window.location.search);
    return urlParams.get('lang') || 'fa';
}

var app = new Vue({
    el: '#app',
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
        language: 'fa',
        showInstallPrompt: false,
        isRefreshing: false,
        deferredPrompt: null
    },
    computed: {
        titles() {
            return translations[this.language].titles;
        },
        labels() {
            return translations[this.language].labels;
        },
        buttons() {
            return translations[this.language].buttons;
        },
        installMessage() {
            return translations[this.language].install.message;
        },
        installButtonText() {
            return translations[this.language].install.button;
        }
    },
    mounted() {
        this.language = getLanguage();
        this.updateDirection();
        this.fetchData();
        setInterval(() => this.fetchData(), 2000);
        
        // Handle install prompt
        window.addEventListener('beforeinstallprompt', (e) => {
            e.preventDefault();
            this.deferredPrompt = e;
            this.showInstallPrompt = true;
        });
    },
    methods: {
        updateDirection() {
            document.documentElement.dir = (this.language === 'ar' || this.language === 'fa') ? 'rtl' : 'ltr';
            document.documentElement.lang = this.language;
        },
        fetchData() {
            axios.get('../Download').then(response => this.Download = response.data).catch(err => console.log(err));
            axios.get('../Upload').then(response => this.Upload = response.data).catch(err => console.log(err));
            axios.get('../MaxSpeed').then(response => this.MaxSpeed = response.data).catch(err => console.log(err));
            axios.get('../Daydownload').then(response => this.Daydownload = response.data).catch(err => console.log(err));
            axios.get('../Dayupload').then(response => this.Dayupload = response.data).catch(err => console.log(err));
            axios.get('../Monthdownload').then(response => this.Monthdownload = response.data).catch(err => console.log(err));
            axios.get('../Monthupload').then(response => this.Monthupload = response.data).catch(err => console.log(err));
            axios.get('../Totaldownload').then(response => this.Totaldownload = response.data).catch(err => console.log(err));
            axios.get('../Totalupload').then(response => this.Totalupload = response.data).catch(err => console.log(err));
        },
        ClickDownload() {
            axios.get('../Download').then(response => this.Download = response.data).catch(err => console.log(err));
        },
        ClickUpdate() {
            axios.get('../Upload').then(response => this.Upload = response.data).catch(err => console.log(err));
        },
        refreshAll() {
            this.isRefreshing = true;
            this.fetchData();
            setTimeout(() => { this.isRefreshing = false; }, 1000);
        },
        async installApp() {
            if (!this.deferredPrompt) return;
            
            this.deferredPrompt.prompt();
            const { outcome } = await this.deferredPrompt.userChoice;
            
            if (outcome === 'accepted') {
                console.log('User accepted the install prompt');
            }
            
            this.deferredPrompt = null;
            this.showInstallPrompt = false;
        },
        dismissInstall() {
            this.showInstallPrompt = false;
        }
    }
});

Vue.config.devtools = true;
