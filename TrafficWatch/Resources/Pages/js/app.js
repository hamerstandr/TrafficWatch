// translations
const translations = {
    fa: {
        titles: {
            trafficMonitor: 'مانیتور ترافیک شبکه',
            realTimeMonitoring: 'نمایش لحظه‌ای ترافیک',
            online: 'آنلاین',
            currentSpeed: 'سرعت فعلی',
            statistics: 'آمار و اطلاعات',
            downloadManager: 'دانلود منیجر',
            downloadManagerStatus: 'وضعیت دانلود منیجر',
            activeDownloads: 'دانلودهای فعال',
            noDownloadManager: 'دانلود منیجر یافت نشد',
            downloadManagerNotEnabled: 'اتصال به دانلود منیجر غیرفعال است'
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
            totalUpload: 'کل آپلود',
            version: 'نسخه',
            queuedDownloads: 'در صف',
            completedDownloads: 'تکمیل شده',
            downloadSpeed: 'سرعت دانلود',
            uploadSpeed: 'سرعت آپلود',
            limits: 'محدودیت‌ها',
            features: 'امکانات',
            scheduler: 'زمان‌بندی',
            clipboardMonitor: 'مانیتور کلیپبورد',
            browserIntegration: 'افزونه مرورگر'
        },
        buttons: {
            refreshDownload: 'بروزرسانی دانلود',
            refreshUpload: 'بروزرسانی آپلود',
            refreshAll: 'بروزرسانی همه',
            enableDownloadManager: 'فعال‌سازی اتصال'
        },
        install: {
            message: 'برنامه را نصب کنید تا دسترسی سریع‌تری داشته باشید',
            button: 'نصب برنامه'
        },
        status: {
            connected: 'متصل',
            disconnected: 'قطع شده',
            running: 'در حال اجرا',
            stopped: 'متوقف شده'
        }
    },
    en: {
        titles: {
            trafficMonitor: 'Network Traffic Monitor',
            realTimeMonitoring: 'Real-time traffic monitoring',
            online: 'Online',
            currentSpeed: 'Current Speed',
            statistics: 'Statistics',
            downloadManager: 'Download Manager',
            downloadManagerStatus: 'Download Manager Status',
            activeDownloads: 'Active Downloads',
            noDownloadManager: 'Download Manager not found',
            downloadManagerNotEnabled: 'Download Manager connection is disabled'
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
            totalUpload: 'Total Upload',
            version: 'Version',
            queuedDownloads: 'Queued',
            completedDownloads: 'Completed',
            downloadSpeed: 'Download Speed',
            uploadSpeed: 'Upload Speed',
            limits: 'Limits',
            features: 'Features',
            scheduler: 'Scheduler',
            clipboardMonitor: 'Clipboard Monitor',
            browserIntegration: 'Browser Integration'
        },
        buttons: {
            refreshDownload: 'Refresh Download',
            refreshUpload: 'Refresh Upload',
            refreshAll: 'Refresh All',
            enableDownloadManager: 'Enable Connection'
        },
        install: {
            message: 'Install the app for quicker access',
            button: 'Install App'
        },
        status: {
            connected: 'Connected',
            disconnected: 'Disconnected',
            running: 'Running',
            stopped: 'Stopped'
        }
    },
    ar: {
        titles: {
            trafficMonitor: 'مراقب حركة الشبكة',
            realTimeMonitoring: 'مراقبة حركة المرور في الوقت الحقيقي',
            online: 'متصل',
            currentSpeed: 'السرعة الحالية',
            statistics: 'الإحصائيات',
            downloadManager: 'مدير التنزيل',
            downloadManagerStatus: 'حالة مدير التنزيل',
            activeDownloads: 'التنزيلات النشطة',
            noDownloadManager: 'لم يتم العثور على مدير التنزيل',
            downloadManagerNotEnabled: 'اتصال مدير التنزيل معطل'
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
            totalUpload: 'إجمالي الرفع',
            version: 'الإصدار',
            queuedDownloads: 'في الانتظار',
            completedDownloads: 'مكتمل',
            downloadSpeed: 'سرعة التنزيل',
            uploadSpeed: 'سرعة الرفع',
            limits: 'الحدود',
            features: 'الميزات',
            scheduler: 'المجدول',
            clipboardMonitor: 'مراقب الحافظة',
            browserIntegration: 'تكامل المتصفح'
        },
        buttons: {
            refreshDownload: 'تحديث التنزيل',
            refreshUpload: 'تحديث الرفع',
            refreshAll: 'تحديث الكل',
            enableDownloadManager: 'تفعيل الاتصال'
        },
        install: {
            message: 'قم بتثبيت التطبيق للوصول بشكل أسرع',
            button: 'تثبيت التطبيق'
        },
        status: {
            connected: 'متصل',
            disconnected: 'غير متصل',
            running: 'يعمل',
            stopped: 'متوقف'
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
        deferredPrompt: null,
        // Download Manager Data
        downloadManagerEnabled: true,
        downloadManager: null,
        downloadManagerLoading: false,
        showDownloadManagerSection: true
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
        },
        statusTexts() {
            return translations[this.language].status;
        },
        dmStatusClass() {
            if (!this.downloadManager) return 'dm-disconnected';
            return this.downloadManager.isRunning ? 'dm-connected' : 'dm-disconnected';
        }
    },
    mounted() {
        this.language = getLanguage();
        this.updateDirection();
        this.fetchData();
        setInterval(() => this.fetchData(), 2000);
        
        // Fetch download manager status
        this.fetchDownloadManagerStatus();
        setInterval(() => this.fetchDownloadManagerStatus(), 5000);
        
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
        fetchDownloadManagerStatus() {
            if (!this.downloadManagerEnabled) {
                this.downloadManager = null;
                return;
            }
            
            this.downloadManagerLoading = true;
            axios.get('../api/downloadmanager/status')
                .then(response => {
                    this.downloadManager = response.data;
                    this.downloadManagerLoading = false;
                })
                .catch(err => {
                    console.log('Download Manager not available:', err);
                    this.downloadManager = null;
                    this.downloadManagerLoading = false;
                });
        },
        formatBytes(bytes) {
            if (!bytes || bytes === 0) return '0 B/s';
            const k = 1024;
            const sizes = ['B/s', 'KB/s', 'MB/s', 'GB/s'];
            const i = Math.floor(Math.log(bytes) / Math.log(k));
            return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
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
            this.fetchDownloadManagerStatus();
            setTimeout(() => { this.isRefreshing = false; }, 1000);
        },
        toggleDownloadManager() {
            this.downloadManagerEnabled = !this.downloadManagerEnabled;
            if (this.downloadManagerEnabled) {
                this.fetchDownloadManagerStatus();
            } else {
                this.downloadManager = null;
            }
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
