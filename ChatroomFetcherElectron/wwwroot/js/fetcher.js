const { ipcRenderer } = require('electron');
const SETTING_KEYS = {
    ECPAY_ID: "ecpayId",
    ECPAY_FAKE_SOURCE: "ecpayFakeSource",
    OPAY_ID: "opayId",
    OPAY_FAKE_SOURCE: "opayFakeSource",
    ONE_COMME_URL: "oneCommeUrl",
    ONE_COMME_TEMPLATE_DIRECTORY: "oneCommeTemplateDirectory"
}

document.addEventListener('DOMContentLoaded', function(){
    let ecpayIdText = document.querySelector("#ecpayId");
    let ecpayFakeSourceCheckbox = document.querySelector("#ecpayFakeSource");
    let opayIdText = document.querySelector("#opayId");
    let opayFakeSourceCheckbox = document.querySelector("#opayFakeSource");
    let oneCommeUrlText = document.querySelector("#onecommeUrl");
    let oneCommeTemplateDirectoryText = document.querySelector("#onecommeTemplateDirectory");
    let connectButton = document.querySelector("#connectButton");

    ecpayIdText.value = localStorage.getItem(SETTING_KEYS.ECPAY_ID) ?? "";
    ecpayFakeSourceCheckbox.checked = (localStorage.getItem(SETTING_KEYS.ECPAY_FAKE_SOURCE) ?? "").toLowerCase() === "checked";
    opayIdText.value = localStorage.getItem(SETTING_KEYS.OPAY_ID) ?? "";
    opayFakeSourceCheckbox.checked = (localStorage.getItem(SETTING_KEYS.OPAY_FAKE_SOURCE) ?? "").toLowerCase() === "checked";
    oneCommeUrlText.value = localStorage.getItem(SETTING_KEYS.ONE_COMME_URL) ?? "";
    oneCommeTemplateDirectoryText.value = localStorage.getItem(SETTING_KEYS.ONE_COMME_TEMPLATE_DIRECTORY) ?? "";
    
    connectButton.addEventListener("click", function(){
        ecpayIdText.disabled = true;
        ecpayFakeSourceCheckbox.disabled = true;
        opayIdText.disabled = true;
        opayFakeSourceCheckbox.disabled = true;
        oneCommeUrlText.disabled = true;
        oneCommeTemplateDirectoryText.disabled = true;
        connectButton.disabled = true;

        let ecpayId = ecpayIdText.value;
        let ecpayFakeSource = ecpayFakeSourceCheckbox.checked;
        let opayId = opayIdText.value;
        let opayFakeSource = opayFakeSourceCheckbox.checked;
        let oneCommeUrl = oneCommeUrlText.value;
        let oneCommeTemplateDirectory = oneCommeTemplateDirectoryText.value;
        
        localStorage.setItem(SETTING_KEYS.ECPAY_ID, ecpayId);
        localStorage.setItem(SETTING_KEYS.ECPAY_FAKE_SOURCE, ecpayFakeSource ? "checked" : "unchecked");
        localStorage.setItem(SETTING_KEYS.OPAY_ID, opayId);
        localStorage.setItem(SETTING_KEYS.OPAY_FAKE_SOURCE, opayFakeSource ? "checked" : "unchecked");
        localStorage.setItem(SETTING_KEYS.ONE_COMME_URL, oneCommeUrl);
        localStorage.setItem(SETTING_KEYS.ONE_COMME_TEMPLATE_DIRECTORY, oneCommeTemplateDirectory);
        ipcRenderer.send('connectToFetch', [
            ecpayId, ecpayFakeSource, 
            opayId, opayFakeSource, 
            oneCommeUrl, oneCommeTemplateDirectory]);
    });
});