/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, Component, ElementRef, Input, OnDestroy, ViewChild } from '@angular/core';
import { ListViewComponent, ResourceLoaderService, WorkflowDto } from '@app/shared';

declare const vis: any;

@Component({
    standalone: true,
    selector: 'sqx-workflow-diagram',
    styleUrls: ['./workflow-diagram.component.scss'],
    templateUrl: './workflow-diagram.component.html',
    imports: [
        ListViewComponent,
    ],
})
export class WorkflowDiagramComponent implements AfterViewInit, OnDestroy {
    private network: any;

    @ViewChild('chartContainer', { static: false })
    public chartContainer!: ElementRef;

    @Input({ required: true })
    public workflow!: WorkflowDto;

    public isLoaded = false;

    constructor(
        private readonly resourceLoader: ResourceLoaderService,
    ) {
    }

    public ngOnDestroy() {
        this.network?.destroy();
    }

    public ngOnChanges() {
        this.updateNetwork();
    }

    public ngAfterViewInit() {
        this.updateNetwork();
    }

    private async updateNetwork() {
        if (!this.chartContainer?.nativeElement || !this.workflow) {
            return;
        }

        await this.resourceLoader.loadLocalScript('dependencies/vis-network/vis-network.min.js');

        const { edges, nodes } = buildGraph(this.workflow);

        this.network?.destroy();
        this.network = new vis.Network(this.chartContainer.nativeElement, { edges, nodes }, GRAPH_OPTIONS);
        this.network.stabilize();
        this.network.fit();

        this.isLoaded = true;
    }
}

function buildGraph(workflow: WorkflowDto) {
    const nodes = new vis.DataSet();

    for (const step of workflow.steps) {
        let label = `<b>${step.name}</b>`;

        if (step.noUpdate) {
            label += '\nPrevent updates';

            if (step.noUpdateExpression) {
                label += `\nwhen (${step.noUpdateExpression})`;
            }

            if (step.noUpdateRoles && step.noUpdateRoles.length > 0) {
                label += `\nfor ${step.noUpdateRoles.join(', ')}`;
            }
        }

        if (step.name === 'Published') {
            label += '\nAvailable in the API';
        }

        const node: any = { id: step.name, label, color: step.color };

        nodes.add(node);
    }

    if (workflow.initial) {
        nodes.add({ id: 0, color: '#000', label: 'Start', shape: 'dot', size: 3 });
    }

    const edges = new vis.DataSet();

    for (const transition of workflow.transitions) {
        let label = '';

        if (transition.expression) {
            label += `\nwhen (${transition.expression})`;
        }

        if (transition.roles && transition.roles.length > 0) {
            label += `\nfor ${transition.roles.join(', ')}`;
        }

        const edge: any = { ...transition, label };

        edges.add(edge);
    }

    if (workflow.initial) {
        edges.add({ from: 0, to: workflow.initial });
    }

    return { edges, nodes };
}

const GRAPH_OPTIONS = {
    nodes: {
        borderWidth: 2,
        font: {
            multi: true,
            align: 'left',
            ital: {
                size: 16,
            },
            bold: {
                size: 20,
            },
            size: 16,
        },
        shape: 'dot',
        shadow: true,
    },
    edges: {
        arrows: 'to',
        font: {
            multi: true,
            ital: {
                size: 16,
            },
            size: 16,
        },
        color: 'gray',
    },
    layout: {
        randomSeed: 2,
    },
    physics: {
        enabled: false,
        repulsion: {
            nodeDistance: 300,
        },
        solver: 'repulsion',
    },
};
