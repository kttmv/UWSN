import { app, BrowserWindow } from 'electron'
import './ipc'

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

// https://stackoverflow.com/questions/45119526/electron-dying-without-any-information-what-now
// handle crashes and kill events
process.on('uncaughtException', function (err) {
    // log the message and stack trace
    // fs.writeFileSync('crash.log', err + '\n' + err.stack)
    console.error(err + '\n' + err.stack)

    // do any cleanup like shutting down servers, etc

    // relaunch the app (if you want)
    // app.relaunch({ args: [] })
    // app.exit(0)
})
