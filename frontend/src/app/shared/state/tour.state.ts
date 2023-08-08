/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Inject, Injectable } from '@angular/core';
import { filter } from 'rxjs';
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

@Injectable()
export class TourState extends State<Snapshot> {
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
        private readonly uiOptions: UIOptions,
    ) {
        super({});

        debug(this, 'tour');

        this.tourService.setDefaults(this.definition.defaults);

        for (const tasks of definition.tasks) {
            tasks.onComplete.subscribe(() => {
                this.next(state => ({
                    ...state,
                    completedTasks: { ...state.completedTasks || {}, [tasks.id]: true },
                }));
            });
        }

        this.changes.pipe(filter(c => c.event !== LoadedEvent))
            .subscribe(change => {
                this.uiState.setCommon('tour', change.snapshot);
            });

        this.uiState.getCommon('tour', {} as Snapshot)
            .subscribe(change => {
                this.next({ ...change, status: change.status || 'Ready' }, LoadedEvent);
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

        this.tourService.run(task.steps);
    }

    public disableAllHints() {
        this.next(s => ({ ...s, shownHints: { ...s.shownHints || {}, all: true } }), 'Disable Hints');
    }

    public disableHint(key: string) {
        this.next(s => ({ ...s, shownHints: { ...s.shownHints || {}, [key]: true } }), 'Disable Hint');
    }

    public shouldShowHint(key: string) {
        return this.snapshot.status !== 'Started' &&
            !this.isDisabled &&
            !this.snapshot.shownHints?.[key] &&
            !this.snapshot.shownHints?.['all'];
    }

    private get isDisabled() {
        return this.uiOptions.get('hideOnboarding');
    }
}

const LoadedEvent = 'Loaded';

function createTasks(tasks: TaskDefinition[], completed?: Record<string, boolean>): TaskSnapshot[] {
    let wasCompleted = false;

    return tasks.map(task => {
        const isCompleted = completed?.[task.id] === true;
        const isActive = wasCompleted && !isCompleted;

        wasCompleted = isCompleted;

        return {
            ...task,
            isCompleted,
            isActive,
        };
    });
}