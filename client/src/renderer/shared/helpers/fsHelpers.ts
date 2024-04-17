import { ReadFileReply } from '../../../main/index'

export function readFile(path: string): Promise<string> {
    return new Promise((resolve, reject) => {
        console.log(window.electronAPI.ipcRenderer)
        window.electronAPI.ipcRenderer.once('read-file-reply', (data) => {
            const arg = data as ReadFileReply

            if (arg.success) {
                resolve(arg.data)
            } else {
                reject(arg.data)
            }
        })

        window.electronAPI.ipcRenderer.send('read-file', path)
    })
}

export function openFile(
    properties: Electron.OpenDialogOptions
): Promise<Electron.OpenDialogReturnValue> {
    return new Promise((resolve) => {
        window.electronAPI.ipcRenderer.once('open-file-reply', (data) => {
            const arg = data as Electron.OpenDialogReturnValue

            resolve(arg)
        })

        window.electronAPI.ipcRenderer.send('open-file', properties)
    })
}

export function saveFile(
    properties: Electron.SaveDialogOptions
): Promise<Electron.SaveDialogReturnValue> {
    return new Promise((resolve) => {
        window.electronAPI.ipcRenderer.once('save-file-reply', (data) => {
            const arg = data as Electron.OpenDialogReturnValue

            resolve(arg)
        })

        window.electronAPI.ipcRenderer.send('save-file', properties)
    })
}
