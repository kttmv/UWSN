import { spawn } from 'child_process'
import {
    app,
    dialog,
    ipcMain,
    IpcMainEvent,
    OpenDialogOptions,
    SaveDialogOptions
} from 'electron'
import fs from 'fs'
import os from 'os'

// -----------------------------------------------------------------------------
// Эти функции нужны для того, чтобы строго соблюдалась типизация каналов.

// Для IPC использовать только эти функции, не вызывать ipcMain.on и
// event.reply вручную!

function on(
    channel: Channels,
    listener: (event: IpcMainEvent, ...args: unknown[]) => void
) {
    ipcMain.on(channel, listener)
}

function reply(event: IpcMainEvent, channel: ReplyChannels, arg: unknown) {
    let str = JSON.stringify(arg)

    if (str.length > 100) {
        str = str.substring(0, 100) + '...'
    }

    console.log(`IPC REPLY (${channel}):\n${str}`)
    event.reply(channel, arg)
}
// -----------------------------------------------------------------------------

on('run-shell', (event, args) => {
    console.log('\nRUN SIMULATOR')
    console.log('ARGS: ', args)

    const isWindows = os.platform() === 'win32'
    const isLinux = os.platform() === 'linux'

    const path = app.isPackaged
        ? isWindows
            ? 'simulator\\UWSN.exe'
            : 'simulator/UWSN'
        : 'dotnet run --project ../UWSN --'

    const child = spawn(`${path} ${args}`, [], {
        shell: true
    })

    child.stderr.on('data', (data) => {
        console.error(`stderr: ${data}`)
        reply(event, 'run-shell-reply', data.toString())
    })

    child.stdout.on('data', (data) => {
        console.log(`stdout: ${data}`)
        reply(event, 'run-shell-reply', data.toString())
    })

    child.on('close', (code) => {
        console.log(`child process exited with code ${code}`)
        reply(event, 'run-shell-close', code)
    })
})

on('read-file', (event, arg) => {
    console.log(arg)

    const path = arg as string

    if (!path) {
        reply(event, 'read-file-reply', {
            error: 'Путь к файлу пуст.'
        } as ReadFileReply)

        return
    }

    fs.stat(path, function (error) {
        if (error) {
            if (error.code === 'ENOENT') {
                reply(event, 'read-file-reply', {
                    error: 'Указанный файл не существует.'
                } as ReadFileReply)
            } else {
                reply(event, 'read-file-reply', {
                    error: `Ошибка при чтении файла: ${error.message}`
                } as ReadFileReply)
            }

            return
        }
    })

    fs.readFile(path, 'utf8', (error, data) => {
        if (error) {
            reply(event, 'read-file-reply', {
                error: `Ошибка при чтении файла: ${error.message}`
            } as ReadFileReply)

            return
        }

        reply(event, 'read-file-reply', {
            content: data
        } as ReadFileReply)
    })
})

on('open-file-dialog', (event, arg) => {
    console.log(arg)

    const data = arg as OpenDialogOptions

    dialog.showOpenDialog(data).then((result) => {
        reply(event, 'open-file-dialog-reply', result)
    })
})

on('save-file-dialog', (event, arg) => {
    console.log(arg)

    const data = arg as SaveDialogOptions

    dialog.showSaveDialog(data).then((result) => {
        reply(event, 'save-file-dialog-reply', result)
    })
})

on('write-file', (event, arg) => {
    console.log(arg)

    const data = arg as WriteFileData

    fs.writeFile(data.path, data.content, (error) => {
        if (error) {
            reply(
                event,
                'write-file-reply',
                `Ошибка при записи файла: ${error.message}`
            )
        } else {
            reply(event, 'write-file-reply', {})
        }
    })
})
