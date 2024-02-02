const { contextBridge, ipcRenderer } = require('electron')

contextBridge.exposeInMainWorld('electron', {
    send: (channel: string, data: string) => {
        ipcRenderer.send(channel, data)
    }
})
