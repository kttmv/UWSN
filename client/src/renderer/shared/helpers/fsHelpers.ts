function log(data: unknown) {
    console.log(`IPC REPLY:\n${JSON.stringify(data)}`)
}

export function readFile(path: string): Promise<string> {
    return new Promise((resolve, reject) => {
        window.electronAPI.ipcRenderer.once('read-file-reply', (arg) => {
            const data = arg as ReadFileReply

            // log(data)

            if (data.error) {
                reject(data.error)
            } else {
                // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
                resolve(data.content!)
            }
        })

        window.electronAPI.ipcRenderer.send('read-file', path)
    })
}

export function writeFile(
    path: string,
    content: string
): Promise<WriteFileReply> {
    return new Promise((resolve, reject) => {
        window.electronAPI.ipcRenderer.once('write-file-reply', (arg) => {
            const data = arg as WriteFileReply

            // log(data)

            if (data.error) {
                reject(data.error)
            } else {
                resolve(data)
            }
        })

        const data: WriteFileData = {
            path,
            content
        }

        window.electronAPI.ipcRenderer.send('write-file', data)
    })
}

export function showOpenFileDialog(
    properties: Electron.OpenDialogOptions
): Promise<Electron.OpenDialogReturnValue> {
    return new Promise((resolve) => {
        window.electronAPI.ipcRenderer.once('open-file-dialog-reply', (arg) => {
            const data = arg as Electron.OpenDialogReturnValue

            // log(data)

            resolve(data)
        })

        window.electronAPI.ipcRenderer.send('open-file-dialog', properties)
    })
}

export function showSaveFileDialog(
    properties: Electron.SaveDialogOptions
): Promise<Electron.SaveDialogReturnValue> {
    return new Promise((resolve) => {
        window.electronAPI.ipcRenderer.once('save-file-dialog-reply', (arg) => {
            const data = arg as Electron.OpenDialogReturnValue

            // log(data)

            resolve(data)
        })

        window.electronAPI.ipcRenderer.send('save-file-dialog', properties)
    })
}
