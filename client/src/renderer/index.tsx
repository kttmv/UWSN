import { ChakraProvider } from '@chakra-ui/react'
import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import App from './app/App'

const container = document.getElementById('root') as HTMLElement
const root = createRoot(container)
root.render(
    <ChakraProvider>
        <StrictMode>
            <App />
        </StrictMode>
    </ChakraProvider>
)

export function executeCommand(command: string) {
    window.electron.ipcRenderer.sendMessage('shell', command)
}
