import { ChakraProvider, ColorModeScript } from '@chakra-ui/react'
import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import App from './App'
import theme from './theme'

const root = createRoot(document.body)
root.render(<Index />)

function Index() {
    return (
        <StrictMode>
            <ColorModeScript initialColorMode={theme.config.initialColorMode} />
            <ChakraProvider theme={theme}>
                <App />
            </ChakraProvider>
        </StrictMode>
    )
}
