import {
    Slider,
    SliderFilledTrack,
    SliderMark,
    SliderThumb,
    SliderTrack,
    Tooltip
} from '@chakra-ui/react'
import { useState } from 'react'
import useViewerSettingsStore from '../store/viewerSettingsStore'

export default function Viewer3DScaleSlider() {
    const [showTooltip, setShowTooltip] = useState(false)

    const { scale, setScale } = useViewerSettingsStore()

    const min = 1
    const max = 2000

    return (
        <Slider
            id='slider'
            defaultValue={scale}
            colorScheme='teal'
            onChange={(v) => setScale(v)}
            onMouseEnter={() => setShowTooltip(true)}
            onMouseLeave={() => setShowTooltip(false)}
            min={min}
            max={max}
        >
            <SliderMark value={min} mt='1' fontSize='sm'>
                1:{min}
            </SliderMark>
            <SliderMark value={max / 2} mt='1' ml='-20px' fontSize='sm'>
                1:{max / 2}
            </SliderMark>
            <SliderMark value={max} mt='1' ml='-35px' fontSize='sm'>
                1:{max}
            </SliderMark>
            <SliderTrack>
                <SliderFilledTrack />
            </SliderTrack>
            <Tooltip
                hasArrow
                bg='teal.500'
                color='white'
                placement='top'
                isOpen={showTooltip}
                label={`1:${scale}`}
            >
                <SliderThumb />
            </Tooltip>
        </Slider>
    )
}
