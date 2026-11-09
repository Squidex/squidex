/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, DestroyRef, effect, ElementRef, inject, input, signal, viewChild } from '@angular/core';
import { ListViewComponent, ResourceLoaderService, ThemeService, WorkflowView } from '@app/shared';

declare const vis: any;

@Component({
    selector: 'sqx-workflow-diagram',
    styleUrls: ['./workflow-diagram.component.scss'],
    templateUrl: './workflow-diagram.component.html',
    imports: [
        ListViewComponent,
    ],
})
export class WorkflowDiagramComponent {
    private readonly destroyRef = inject(DestroyRef);
    private readonly resourceLoader = inject(ResourceLoaderService);
    private readonly themeService = inject(ThemeService);
    private network: any;

    /** The workflow to visualize as a diagram. */
    public readonly workflow = input.required<WorkflowView>();

    protected readonly chartContainer = viewChild<ElementRef<HTMLElement>>('chartContainer');

    protected readonly isLoaded = signal(false);

    constructor() {
        const destroyRef = this.destroyRef;
        const themeService = this.themeService;

        effect(() => {
            themeService.theme();

            const container = this.chartContainer()?.nativeElement;
            if (!container) {
                return;
            }

            this.updateNetwork(container, this.workflow());
        });

        destroyRef.onDestroy(() => {
            this.network?.destroy();
        });
    }

    private async updateNetwork(container: HTMLElement, workflow: WorkflowView) {
        await this.resourceLoader.loadLocalScript('dependencies/vis-network/vis-network.min.js');

        const colors = {
            edge: this.themeService.getColor('diagram-edge'),
            start: this.themeService.getColor('diagram-start'),
            text: this.themeService.getColor('diagram-text'),
            textStroke: this.themeService.getColor('diagram-text-stroke'),
        };

        const { edges, nodes } = buildGraph(workflow, colors);

        this.network?.destroy();
        this.network = new vis.Network(container, { edges, nodes }, buildOptions(colors));
        this.network.stabilize();
        this.network.fit();

        this.isLoaded.set(true);
    }
}

type DiagramColors = { edge: string; start: string; text: string; textStroke: string };

function buildGraph(workflow: WorkflowView, colors: DiagramColors) {
    const nodes = new vis.DataSet();

    for (const step of workflow.steps) {
        let label = `<b>${step.name}</b>`;

        if (step.values.noUpdate) {
            label += '\nPrevent updates';

            if (step.values.noUpdateExpression) {
                label += `\nwhen (${step.values.noUpdateExpression})`;
            }

            if (step.values.noUpdateRoles && step.values.noUpdateRoles.length > 0) {
                label += `\nfor ${step.values.noUpdateRoles.join(', ')}`;
            }
        }

        if (step.name === 'Published') {
            label += '\nAvailable in the API';
        }

        const node: any = { id: step.name, label, color: step.values.color };

        nodes.add(node);
    }

    if (workflow.dto.initial) {
        nodes.add({ id: 0, color: colors.start, label: 'Start', shape: 'dot', size: 3 });
    }

    const edges = new vis.DataSet();

    for (const transition of workflow.transitions) {
        let label = '';

        if (transition.values.expression) {
            label += `\nwhen (${transition.values.expression})`;
        }

        if (transition.values.roles && transition.values.roles.length > 0) {
            label += `\nfor ${transition.values.roles.join(', ')}`;
        }

        const edge: any = { ...transition, label };

        edges.add(edge);
    }

    if (workflow.dto.initial) {
        edges.add({ from: 0, to: workflow.dto.initial });
    }

    return { edges, nodes };
}

const buildOptions = (colors: DiagramColors) => ({
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
            color: colors.text,
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
            color: colors.text,
            size: 16,
            strokeColor: colors.textStroke,
        },
        color: colors.edge,
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
});
