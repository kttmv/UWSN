import { ChakraProvider, ColorModeScript } from '@chakra-ui/react'
import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import App from './app/App'
import theme from './app/theme'

const container = document.getElementById('root') as HTMLElement
const root = createRoot(container)
root.render(
    <>
        <StrictMode>
            <ColorModeScript initialColorMode={theme.config.initialColorMode} />
            <ChakraProvider theme={theme}>
                <App />
            </ChakraProvider>
        </StrictMode>
    </>
)

export function runSimulatorShell(args: string) {
    window.electron.ipcRenderer.sendMessage('run-simulator', args)
}
