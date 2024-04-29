import { ElectronHandler } from './main/preload'

// В этом файле определены типы, которые используются для коммуникации между
// процессами render и main

declare global {
    interface Window {
        electronAPI: ElectronHandler
    }
    type Channels =
        | 'run-simulator'
        | 'read-file'
        | 'open-file-dialog'
        | 'save-file-dialog'
        | 'write-file'

    type ReplyChannels =
        | 'run-simulator-reply'
        | 'run-simulator-close'
        | 'read-file-reply'
        | 'open-file-dialog-reply'
        | 'save-file-dialog-reply'
        | 'write-file-reply'

    type ReadFileReply = {
        error: undefined | string
        content: undefined | string
    }

    type WriteFileData = {
        path: string
        content: string
    }

    type WriteFileReply = {
        error: undefined | string
    }
}

export {}
