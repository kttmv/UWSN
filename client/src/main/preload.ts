// Disable no-unused-vars, broken for spread args
/* eslint no-unused-vars: off */
import { contextBridge, ipcRenderer, IpcRendererEvent } from 'electron'

export type Channels =
    | 'run-simulator'
    | 'simulator-reply'
    | 'read-file'
    | 'read-file-reply'
    | 'open-file'
    | 'open-file-reply'
    | 'save-file'
    | 'save-file-reply'

const electronHandler = {
    ipcRenderer: {
        send(channel: Channels, ...args: unknown[]) {
            ipcRenderer.send(channel, ...args)
        },
        on(channel: Channels, func: (...args: unknown[]) => void) {
            const subscription = (
                _event: IpcRendererEvent,
                ...args: unknown[]
            ) => func(...args)
            ipcRenderer.on(channel, subscription)

            return () => {
                ipcRenderer.removeListener(channel, subscription)
            }
        },
        once(channel: Channels, func: (...args: unknown[]) => void) {
            ipcRenderer.once(channel, (_event, ...args) => func(...args))
        }
    }
}

contextBridge.exposeInMainWorld('electron', electronHandler)

export type ElectronHandler = typeof electronHandler
