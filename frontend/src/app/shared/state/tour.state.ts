/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Inject, Injectable } from '@angular/core';
import { debounceTime } from 'rxjs';
import { debug, State, TourService } from '@app/framework';
import { TASK_CONFIGURATION, TaskConfiguration, TaskDefinition } from './tour.tasks';
import { UIState } from './ui.state';

export interface TaskSnapshot extends TaskDefinition {
    // True if active.
    isActive: boolean;

    // True if completed.
    isCompleted: boolean;
}

interface Snapshot {
    // The running task.
    taskId?: string;

    // The completed tasks.
    completedTasks?: Record<string, boolean>;

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
        this.projectFrom(this.tasks, x => x.filter(t => !t.onComplete).length);

    constructor(
        @Inject(TASK_CONFIGURATION) private readonly definition: TaskConfiguration,
        private readonly tourService: TourService,
        private readonly uiState: UIState,
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

        this.changes.pipe(debounceTime(1000))
            .subscribe(change => {
                this.uiState.setCommon('tour', change.snapshot);
            });

        this.uiState.getCommon('tour', {} as Snapshot)
            .subscribe(change => {
                this.next({ ...change, status: change.status || 'Ready' });
            });
    }

    public complete() {
        if (this.isDoneOrNotReady()) {
            return;
        }

        this.next({ status: 'Completed' });
    }

    public start() {
        if (this.snapshot.status !== 'Ready') {
            return;
        }

        this.next({ status: 'Started' });
    }

    public startFirstTask() {
        this.runTask(this.definition.tasks[0]);
    }

    public runTask(task: TaskDefinition | undefined) {
        if (!task || this.snapshot.status !== 'Started') {
            return;
        }

        this.next({ taskId: task.id });

        this.tourService.initialize(task.steps);
        this.tourService.start();
    }

    private isDoneOrNotReady() {
        return !this.snapshot.status || this.snapshot.status === 'Completed';
    }
}

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