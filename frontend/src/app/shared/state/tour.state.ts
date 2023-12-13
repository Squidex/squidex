/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { inject, Inject, Injectable } from '@angular/core';
import { filter, skip, take } from 'rxjs';
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
    private readonly isDisabled = inject(UIOptions).value.hideOnboarding;

    public completedTasks =
        this.project(x => x.completedTasks);

    public status =
        this.project(x => x.status);

    public tasks =
        this.projectFrom(this.completedTasks, x => createTasks(this.definition.tasks, x));

    public pendingTasks =
        this.projectFrom(this.tasks, x => x.filter(t => !t.isCompleted).length);

    constructor(
        @Inject(TASK_CONFIGURATION) private readonly definition: TaskConfiguration,
        private readonly tourService: TourService,
        private readonly uiState: UIState,
    ) {
        super({});

        debug(this, 'tour');

        this.changes.pipe(skip(1), filter(c => c.event !== LoadedEvent))
            .subscribe(change => {
                this.uiState.setCommon('tour', change.snapshot);
            });

        this.uiState.getCommon('tour', {} as Snapshot).pipe(take(1))
            .subscribe(change => {
                this.next({ ...change, status: change.status || 'Ready' }, LoadedEvent);

                for (const tasks of definition.tasks) {
                    tasks.onComplete.subscribe(() => {
                        this.next(state => ({
                            ...state,
                            completedTasks: { ...state.completedTasks || {}, [tasks.id]: true },
                        }));
                    });
                }
            });

            this.tourService.setDefaults(this.definition.defaults);
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

        this.tourService.run(task.steps);
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