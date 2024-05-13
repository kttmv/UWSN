import { ClusterizationAlgorithmType } from './clusterizationAlogirthmType'

export type RetardedClusterization = {
    $type: ClusterizationAlgorithmType.RetardedClusterization
    XClusterCount: number
    ZClusterCount: number
}

export function isRetardedClusterization(
    obj: any
): obj is RetardedClusterization {
    return obj.$type === ClusterizationAlgorithmType.RetardedClusterization
}

export type ClusterizationAlgorithm = RetardedClusterization // | ... (другие типы кластеризации)
