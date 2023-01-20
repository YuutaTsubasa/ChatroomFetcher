const { ipcRenderer } = require('electron');
const SETTING_KEYS = {
    SHOULD_BACKUP: "shouldBackup",
    YOUTUBE_LIVE_ID: "youtubeLiveId",
    ECPAY_ID: "ecpayId",
    ECPAY_FAKE_SOURCE: "ecpayFakeSource",
    OPAY_ID: "opayId",
    OPAY_FAKE_SOURCE: "opayFakeSource",
    ONE_COMME_URL: "oneCommeUrl",
    ONE_COMME_TEMPLATE_DIRECTORY: "oneCommeTemplateDirectory"
}

function stringFormat(format) {
    var args = Array.prototype.slice.call(arguments, 1);
    return format.replace(/{(\d+)}/g, function(match, number) {
        return typeof args[number] != 'undefined'
            ? args[number]
            : match;
    });
}

function registerMessageEvent(){
    let messageList = document.querySelector("#messageList");
    let listTemplate = `
    <li class="{3}" style="border-left: 5px solid rgb({7})">
        <section class="nameSection">
            {0}
            <strong class="name">{1}</strong>
        </section>
        <section class="contentSection">
        {2}{4}
        </section>
        <section class="informationSection">
        {5} {6}
        </section>
    </li>
    `;

    ipcRenderer.on("message", function(event, newMessageJsonString) {
        let newMessages = JSON.parse(newMessageJsonString);
        for (let message of newMessages) {
            let time = new Date(message.data.timestamp);
            let timeISOString = new Date(time.getTime() - (time.getTimezoneOffset() * 60000))
                .toISOString().split("T");
            
            messageList.innerHTML = stringFormat(
                listTemplate,
                message.data.profileImage 
                    ? `<img src="${message.data.profileImage}" class="avatar"/>`
                    : "",
                message.data.name,
                message.data.comment,
                message.service,
                message.data.paidText 
                    ? `<br/><strong class="donateText">(Donate: ${message.data.paidText})</strong>`
                    : "",
                `${timeISOString[0]} ${timeISOString[1].split(".")[0]}`,
                message.name,
                `${message.color.r},${message.color.g},${message.color.b}`)
                + messageList.innerHTML;
        }
    });
}

document.addEventListener('DOMContentLoaded', function(){
    let backupFilePathFileInput = document.querySelector("#backupFilePath");
    let shouldBackupCheckbox = document.querySelector("#shouldBackup");
    let youtubeLiveIdText = document.querySelector("#youtubeLiveId");
    let ecpayIdText = document.querySelector("#ecpayId");
    let ecpayFakeSourceCheckbox = document.querySelector("#ecpayFakeSource");
    let opayIdText = document.querySelector("#opayId");
    let opayFakeSourceCheckbox = document.querySelector("#opayFakeSource");
    let oneCommeUrlText = document.querySelector("#onecommeUrl");
    let oneCommeTemplateDirectoryText = document.querySelector("#onecommeTemplateDirectory");
    let connectButton = document.querySelector("#connectButton");

    ipcRenderer.on("loadConfig", function(event, configString) {
        let config = JSON.parse(configString);
        shouldBackupCheckbox.checked = (config[SETTING_KEYS.SHOULD_BACKUP] ?? "checked").toLowerCase() === "checked";
        youtubeLiveIdText.value = config[SETTING_KEYS.YOUTUBE_LIVE_ID] ?? "";
        ecpayIdText.value = config[SETTING_KEYS.ECPAY_ID] ?? "";
        ecpayFakeSourceCheckbox.checked = (config[SETTING_KEYS.ECPAY_FAKE_SOURCE] ?? "").toLowerCase() === "checked";
        opayIdText.value = config[SETTING_KEYS.OPAY_ID] ?? "";
        opayFakeSourceCheckbox.checked = (config[SETTING_KEYS.OPAY_FAKE_SOURCE] ?? "").toLowerCase() === "checked";
        oneCommeUrlText.value = config[SETTING_KEYS.ONE_COMME_URL] ?? "";
        oneCommeTemplateDirectoryText.value = config[SETTING_KEYS.ONE_COMME_TEMPLATE_DIRECTORY] ?? "";
    });
    
    
    connectButton.addEventListener("click", function(){
        backupFilePathFileInput.disabled = true;
        shouldBackupCheckbox.disabled = true;
        youtubeLiveIdText.disabled = true;
        ecpayIdText.disabled = true;
        ecpayFakeSourceCheckbox.disabled = true;
        opayIdText.disabled = true;
        opayFakeSourceCheckbox.disabled = true;
        oneCommeUrlText.disabled = true;
        oneCommeTemplateDirectoryText.disabled = true;
        connectButton.disabled = true;

        let backupFilePath = backupFilePathFileInput.files[0].path;
        let shouldBackup = shouldBackupCheckbox.checked;
        let youtubeLiveId = youtubeLiveIdText.value;
        let ecpayId = ecpayIdText.value;
        let ecpayFakeSource = ecpayFakeSourceCheckbox.checked;
        let opayId = opayIdText.value;
        let opayFakeSource = opayFakeSourceCheckbox.checked;
        let oneCommeUrl = oneCommeUrlText.value;
        let oneCommeTemplateDirectory = oneCommeTemplateDirectoryText.value;
        
        let config = {};
        config[SETTING_KEYS.SHOULD_BACKUP] = shouldBackup ? "checked" : "unchecked";
        config[SETTING_KEYS.YOUTUBE_LIVE_ID] = youtubeLiveId;
        config[SETTING_KEYS.ECPAY_ID] = ecpayId;
        config[SETTING_KEYS.ECPAY_FAKE_SOURCE] = ecpayFakeSource ? "checked" : "unchecked";
        config[SETTING_KEYS.OPAY_ID] = opayId;
        config[SETTING_KEYS.OPAY_FAKE_SOURCE] = opayFakeSource ? "checked" : "unchecked";
        config[SETTING_KEYS.ONE_COMME_URL] = oneCommeUrl;
        config[SETTING_KEYS.ONE_COMME_TEMPLATE_DIRECTORY] = oneCommeTemplateDirectory;
        
        ipcRenderer.send("saveConfig", JSON.stringify(config));
        ipcRenderer.send('connectToFetch', [
            backupFilePath, shouldBackup,
            youtubeLiveId,
            ecpayId, ecpayFakeSource, 
            opayId, opayFakeSource, 
            oneCommeUrl, oneCommeTemplateDirectory]);
    });
    
    registerMessageEvent();
});