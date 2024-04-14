import { ReadFileReply } from '../../../main/main'

export function readFile(path: string): Promise<string> {
    return new Promise((resolve, reject) => {
        window.electron.ipcRenderer.once('read-file-reply', (data) => {
            let arg = data as ReadFileReply

            if (arg.success) {
                resolve(arg.data)
            } else {
                reject(arg.data)
            }
        })

        window.electron.ipcRenderer.send('read-file', path)
    })
}

export function openFile(
    properties: Electron.OpenDialogOptions
): Promise<Electron.OpenDialogReturnValue> {
    return new Promise((resolve) => {
        window.electron.ipcRenderer.once('open-file-reply', (data) => {
            let arg = data as Electron.OpenDialogReturnValue

            resolve(arg)
        })

        window.electron.ipcRenderer.send('open-file', properties)
    })
}

export function saveFile(
    properties: Electron.SaveDialogOptions
): Promise<Electron.SaveDialogReturnValue> {
    return new Promise((resolve) => {
        window.electron.ipcRenderer.once('save-file-reply', (data) => {
            let arg = data as Electron.OpenDialogReturnValue

            resolve(arg)
        })

        window.electron.ipcRenderer.send('save-file', properties)
    })
}
