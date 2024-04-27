import { contextBridge, ipcRenderer, IpcRendererEvent } from 'electron'

const electronHandler = {
    ipcRenderer: {
        send(channel: Channels, ...args: unknown[]) {
            ipcRenderer.send(channel, ...args)
        },
        on(channel: ReplyChannels, func: (...args: unknown[]) => void) {
            const subscription = (
                _event: IpcRendererEvent,
                ...args: unknown[]
            ) => func(...args)
            ipcRenderer.on(channel, subscription)

            return () => {
                ipcRenderer.removeListener(channel, subscription)
            }
        },
        once(channel: ReplyChannels, func: (...args: unknown[]) => void) {
            ipcRenderer.once(channel, (_event, ...args) => func(...args))
        }
    }
}

contextBridge.exposeInMainWorld('electronAPI', electronHandler)

export type ElectronHandler = typeof electronHandler
