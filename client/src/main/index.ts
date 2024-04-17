import { spawn } from 'child_process'
import { app, BrowserWindow, dialog, ipcMain } from 'electron'
import fs from 'fs'

declare const MAIN_WINDOW_WEBPACK_ENTRY: string
declare const MAIN_WINDOW_PRELOAD_WEBPACK_ENTRY: string

// Handle creating/removing shortcuts on Windows when installing/uninstalling.
if (require('electron-squirrel-startup')) {
    app.quit()
}

const createWindow = (): void => {
    // Create the browser window.
    const mainWindow = new BrowserWindow({
        height: 600,
        width: 800,
        webPreferences: {
            // preload: path.join(__dirname, 'preload.js')
            preload: MAIN_WINDOW_PRELOAD_WEBPACK_ENTRY
        }
    })

    // and load the index.html of the app.
    mainWindow.loadURL(MAIN_WINDOW_WEBPACK_ENTRY)
}

// This method will be called when Electron has finished
// initialization and is ready to create browser windows.
// Some APIs can only be used after this event occurs.
app.on('ready', createWindow)

// Quit when all windows are closed, except on macOS. There, it's common
// for applications and their menu bar to stay active until the user quits
// explicitly with Cmd + Q.
app.on('window-all-closed', () => {
    if (process.platform !== 'darwin') {
        app.quit()
    }
})

app.on('activate', () => {
    // On OS X it's common to re-create a window in the app when the
    // dock icon is clicked and there are no other windows open.
    if (BrowserWindow.getAllWindows().length === 0) {
        createWindow()
    }
})

// In this file you can include the rest of your app's specific main process
// code. You can also put them in separate files and import them here.
ipcMain.on('run-simulator', (event, args) => {
    const path = app.isPackaged
        ? '.\\'
        : '..\\UWSN\\bin\\Debug\\net7.0\\UWSN.exe'

    const child = spawn(`${path} ${args}`, [], {
        shell: true
    })

    child.stdout.on('data', (data) => {
        console.log(`stdout: ${data}`)
        // event.reply('shell-reply', { isError: false, reply: data.toString() })
        event.reply('simulator-reply', data.toString())
    })

    child.stderr.on('data', (data) => {
        console.error(`stderr: ${data}`)
        // event.reply('shell-reply', { isError: true, reply: data.toString() })
        event.reply('simulator-reply', data.toString())
    })

    child.on('close', (code) => {
        console.log(`child process exited with code ${code}`)
    })
})

export type ReadFileReply = {
    success: boolean
    data: string
}

ipcMain.on('read-file', (event, path) => {
    if (!path) {
        event.reply('read-file-reply', {
            success: false,
            data: 'Путь к файлу пуст.'
        } as ReadFileReply)

        return
    }

    fs.stat(path, function (err, stat) {
        if (err) {
            if (err.code === 'ENOENT') {
                event.reply('read-file-reply', {
                    success: false,
                    data: 'Указанный файл не существует.'
                } as ReadFileReply)
            } else {
                event.reply('read-file-reply', {
                    success: false,
                    data: `Ошибка при чтении файла: ${err.message}`
                } as ReadFileReply)
            }

            return
        }
    })

    fs.readFile(path, 'utf8', (err, data) => {
        if (err) {
            event.reply('read-file-reply', {
                success: false,
                data: `Ошибка при чтении файла: ${err.message}`
            } as ReadFileReply)

            return
        }

        event.reply('read-file-reply', { success: true, data } as ReadFileReply)
    })
})

ipcMain.on('open-file', (event, data) => {
    dialog.showOpenDialog(data).then((result) => {
        event.reply('open-file-reply', result)
    })
})

ipcMain.on('save-file', (event, data) => {
    dialog.showSaveDialog(data).then((filePath) => {
        event.reply('save-file-reply', filePath)
    })
})
