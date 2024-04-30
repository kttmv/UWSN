import {
    Button,
    Popover,
    PopoverArrow,
    PopoverBody,
    PopoverCloseButton,
    PopoverContent,
    PopoverHeader,
    PopoverTrigger,
    Text
} from '@chakra-ui/react'
import { IconSettings } from '@tabler/icons-react'
import Viewer3DScaleSlider from './Viewer3DScaleSlider'

export default function Viewer3DSettings() {
    return (
        <Popover>
            <PopoverTrigger>
                <Button position='absolute' top='5px' left='5px'>
                    <IconSettings />
                </Button>
            </PopoverTrigger>
            <PopoverContent>
                <PopoverArrow />
                <PopoverCloseButton />
                <PopoverHeader> Настройки </PopoverHeader>
                <PopoverBody>
                    <Text>Масштаб</Text>
                    <Viewer3DScaleSlider />
                </PopoverBody>
            </PopoverContent>
        </Popover>
    )
}
