/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { effect, EnvironmentInjector, inject, Inject, Injectable, runInInjectionContext, untracked } from '@angular/core';
import { debug, State, TourService, UIOptions } from '@app/framework';
import { TASK_CONFIGURATION, TaskConfiguration, TaskDefinition } from './tour.tasks';
import { UIState } from './ui.state';

export interface TaskSnapshot extends TaskDefinition {
    // True if active.
    isActive: boolean;

    // True if completed.
    isCompleted: boolean;
}

interface Snapshot {
    // The completed tasks.
    completedTasks?: Record<string, boolean>;

    // The shown hints.
    shownHints?: { [name: string]: boolean };

    // True, if tour is running.
    status?: 'Ready' | 'Started' | 'Completed';
}

@Injectable({
    providedIn: 'root',
})
export class TourState extends State<Snapshot> {
    private readonly uiOptions = inject(UIOptions);
    private readonly injector = inject(EnvironmentInjector);
    private readonly isDisabled = this.uiOptions.value.hideOnboarding;

    public readonly completedTasks =
        this.project(x => x.completedTasks);

    public readonly status =
        this.project(x => x.status);

    public readonly tasks =
        this.projectFrom(this.completedTasks, x => createTasks(this.definition.tasks, x));

    public readonly pendingTasks =
        this.projectFrom(this.tasks, x => x.filter(t => !t.isCompleted).length);

    constructor(
        @Inject(TASK_CONFIGURATION) private readonly definition: TaskConfiguration,
        private readonly tourService: TourService,
        private readonly uiState: UIState,
    ) {
        super({});

        debug(this, 'tour');

        effect(() => {
            const change = this.lastChange();
            if (change.event === 'Initial' || change.event === LoadedEvent) {
                return;
            }

            untracked(() => this.uiState.setCommon('tour', change.snapshot));
        });

        this.uiState.waitForCommon('tour', {} as Snapshot)
            .subscribe(change => {
                this.next({ ...change, status: change.status || 'Ready' }, LoadedEvent);

                for (const tasks of definition.tasks) {
                    runInInjectionContext(this.injector, tasks.onComplete).subscribe(() => {
                        this.next(state => ({
                            ...state,
                            completedTasks: { ...state.completedTasks || {}, [tasks.id]: true },
                        }));
                    });
                }
            });
    }

    public complete() {
        this.next({ status: 'Completed' }, 'Complete');
    }

    public start() {
        if (this.snapshot.status !== 'Ready' || this.isDisabled) {
            return;
        }

        this.next({ status: 'Started' }, 'Start');
    }

    public runTask(task: TaskDefinition) {
        if (this.snapshot.status !== 'Started') {
            return;
        }

        this.tourService.initialize(task.steps);
        this.tourService.start();
    }

    public disableAllHints() {
        this.disableHintCore('all');
    }

    public disableHint(key: string) {
        this.disableHintCore(key);
    }

    public shouldShowHint(key: string) {
        return this.snapshot.status !== 'Started' &&
            !this.isDisabled &&
            !this.snapshot.shownHints?.[key] &&
            !this.snapshot.shownHints?.['all'];
    }

    private disableHintCore(key: string) {
        this.next(s => ({ ...s, shownHints: { ...s.shownHints || {}, [key]: true } }), 'Disable Hint');
    }
}

const LoadedEvent = 'Loaded';

function createTasks(tasks: TaskDefinition[], completed?: Record<string, boolean>): TaskSnapshot[] {
    const snapshots = tasks.map(task => ({
        ...task,
        isCompleted: completed?.[task.id] === true,
        isActive: false,
    }));

    for (const snapshot of snapshots) {
        snapshot.isActive = !snapshot.isCompleted && snapshots.find(x => x.id === snapshot.dependsOn)?.isCompleted !== false;
    }

    return snapshots;
}