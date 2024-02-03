/* eslint global-require: off, no-console: off, promise/always-return: off */

/**
 * This module executes inside of electron's main process. You can start
 * electron renderer process from here and communicate with the other processes
 * through IPC.
 *
 * When running `npm run build` or `npm run build:main`, this file is compiled to
 * `./src/main.js` using webpack. This gives us some performance wins.
 */
import { spawn } from 'child_process'
import { BrowserWindow, app, ipcMain, shell } from 'electron'
import log from 'electron-log'
import { autoUpdater } from 'electron-updater'
import path from 'path'
import MenuBuilder from './menu'
import { resolveHtmlPath } from './util'
import fs from 'fs'

class AppUpdater {
    constructor() {
        log.transports.file.level = 'info'
        autoUpdater.logger = log
        autoUpdater.checkForUpdatesAndNotify()
    }
}

let mainWindow: BrowserWindow | null = null

ipcMain.on('ipc-example', async (event, arg) => {
    const msgTemplate = (pingPong: string) => `IPC test: ${pingPong}`
    console.log(msgTemplate(arg))
    event.reply('ipc-example', msgTemplate('pong'))
})

if (process.env.NODE_ENV === 'production') {
    const sourceMapSupport = require('source-map-support')
    sourceMapSupport.install()
}

const isDebug =
    process.env.NODE_ENV === 'development' || process.env.DEBUG_PROD === 'true'

if (isDebug) {
    require('electron-debug')()
}

const installExtensions = async () => {
    const installer = require('electron-devtools-installer')
    const forceDownload = !!process.env.UPGRADE_EXTENSIONS
    const extensions = ['REACT_DEVELOPER_TOOLS']

    return installer
        .default(
            extensions.map((name) => installer[name]),
            forceDownload
        )
        .catch(console.log)
}

const createWindow = async () => {
    if (isDebug) {
        await installExtensions()
    }

    const RESOURCES_PATH = app.isPackaged
        ? path.join(process.resourcesPath, 'assets')
        : path.join(__dirname, '../../assets')

    const getAssetPath = (...paths: string[]): string => {
        return path.join(RESOURCES_PATH, ...paths)
    }

    mainWindow = new BrowserWindow({
        show: false,
        width: 1024,
        height: 728,
        minWidth: 600,
        minHeight: 600,
        icon: getAssetPath('icon.png'),
        webPreferences: {
            preload: app.isPackaged
                ? path.join(__dirname, 'preload.js')
                : path.join(__dirname, '../../.erb/dll/preload.js')
        }
    })

    mainWindow.loadURL(resolveHtmlPath('index.html'))

    mainWindow.on('ready-to-show', () => {
        if (!mainWindow) {
            throw new Error('"mainWindow" is not defined')
        }
        if (process.env.START_MINIMIZED) {
            mainWindow.minimize()
        } else {
            mainWindow.show()
        }
    })

    mainWindow.on('closed', () => {
        mainWindow = null
    })

    const menuBuilder = new MenuBuilder(mainWindow)
    menuBuilder.buildMenu()

    // Open urls in the user's browser
    mainWindow.webContents.setWindowOpenHandler((edata) => {
        shell.openExternal(edata.url)
        return { action: 'deny' }
    })

    // Remove this if your app does not use auto updates
    // eslint-disable-next-line
    new AppUpdater()
}

/**
 * Add event listeners...
 */

app.on('window-all-closed', () => {
    // Respect the OSX convention of having the application in memory even
    // after all windows have been closed
    if (process.platform !== 'darwin') {
        app.quit()
    }
})

app.whenReady()
    .then(() => {
        createWindow()
        app.on('activate', () => {
            // On macOS it's common to re-create a window in the app when the
            // dock icon is clicked and there are no other windows open.
            if (mainWindow === null) createWindow()
        })
    })
    .catch(console.log)

ipcMain.on('shell', (event, arg) => {
    console.log(arg)
    let child = spawn(arg, [], {
        shell: true
    })

    child.stdout.on('data', (data) => {
        console.log(`stdout: ${data}`)
        // event.reply('shell-reply', { isError: false, reply: data.toString() })
        event.reply('shell-reply', data.toString())
    })

    child.stderr.on('data', (data) => {
        console.error(`stderr: ${data}`)
        // event.reply('shell-reply', { isError: true, reply: data.toString() })
        event.reply('shell-reply', data.toString())
    })

    child.on('close', (code) => {
        console.log(`child process exited with code ${code}`)
    })
})

ipcMain.on('read-file', (event, path) => {
    fs.readFile(path, 'utf8', (err, data) => {
        if (err) {
            console.error('An error occurred:', err)
            event.reply('read-file-reply', { success: false, data: err })
            return
        }

        event.reply('read-file-reply', { success: true, data })
    })
})