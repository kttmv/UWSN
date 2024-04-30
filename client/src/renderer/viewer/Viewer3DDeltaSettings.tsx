import { Card, useBreakpointValue } from '@chakra-ui/react'
import Viewer3DDeltaSlider from './Viewer3DDeltaSlider'

export default function Viewer3DDeltaSettings() {
    const bottom = useBreakpointValue({
        base: '25px',
        lg: '0px'
    })

    const width = useBreakpointValue({
        base: 'calc(100% - 10px)',
        lg: 'calc(100% - 34px)'
    })

    return (
        <Card
            position='absolute'
            bottom={bottom}
            left='0px'
            width={width}
            margin='5px'
            padding='10px'
        >
            <Viewer3DDeltaSlider />
        </Card>
    )
}
