import { SensorPlacementDistributionType } from '../tabs/SensorPlacement'

export function runSimulatorShell(args: string) {
    window.electronAPI.ipcRenderer.send('run-simulator', args)
}

export function runPlaceSensorsRandomStep(
    distributionType: SensorPlacementDistributionType,
    countX: number,
    countY: number,
    countZ: number,
    uniformA: number,
    uniformB: number,
    projectPath: string
) {
    const distr = SensorPlacementDistributionType[distributionType]

    switch (Number(distributionType)) {
        case SensorPlacementDistributionType.Normal: {
            runSimulatorShell(
                `placeSensorsRndStep ${distr} ` +
                    `${countX} ${countY} ${countZ} ` +
                    `-f "${projectPath}"`
            )
            break
        }
        case SensorPlacementDistributionType.Uniform: {
            runSimulatorShell(
                `placeSensorsRndStep ${distr} ` +
                    `${countX} ${countY} ${countZ} ${uniformA} ${uniformB} 32 ` +
                    `-f "${projectPath}"`
            )
            break
        }
        default: {
            throw new Error('Что-то пошло не так')
        }
    }
}
