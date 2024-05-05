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
import ViewerScaleSlider from './ViewerScaleSlider'

export default function ViewerSettings() {
    return (
        <Popover>
            <PopoverTrigger>
                <Button position='absolute' top='5px' left='5px'>
                    <IconSettings />
                </Button>
            </PopoverTrigger>
            <PopoverContent padding='10px'>
                <PopoverArrow />
                <PopoverCloseButton />
                <PopoverHeader> Настройки </PopoverHeader>
                <PopoverBody>
                    <Text>Масштаб</Text>
                    <ViewerScaleSlider />
                </PopoverBody>
            </PopoverContent>
        </Popover>
    )
}
