export function readFile(path: string): Promise<string> {
    return new Promise((resolve, reject) => {
        window.electron.ipcRenderer.once('read-file-reply', (arg) => {
            if (arg.success) {
                resolve(arg.data)
            } else {
                reject(arg.data)
            }
        })

        window.electron.ipcRenderer.sendMessage('read-file', path)
    })
}
