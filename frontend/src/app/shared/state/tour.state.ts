/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { debounceTime } from 'rxjs';
import { State, StepDefinition, TourService, waitForAnchor } from '@app/framework';
import { UIState } from './ui.state';

export interface TaskDefinition {
    // The ID to store it in the backend.
    id: string;

    // The title.
    title: string;

    // The description.
    description: string;

    // The steps to operate.
    steps: StepDefinition[];
}

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
        this.projectFrom(this.completedTasks, createTasks);

    public pendingTasks =
        this.projectFrom(this.tasks, x => x.filter(t => !t.isCompleted).length);

    constructor(
        private readonly tourService: TourService,
        private readonly uiState: UIState,
    ) {
        super({}, 'Tour');

        this.tourService.setDefaults(DEFAULTS);

        tourService.end$
            .subscribe(() => {
                this.next(state => ({
                    ...state,
                    completedTasks: { ...state.completedTasks || {}, [state.taskId!]: true },
                }));
            });

        this.changes.pipe(debounceTime(1000))
            .subscribe(changes => {
                this.uiState.setCommon('tour', changes);
            });

        this.uiState.getCommon('tour', {} as Snapshot)
            .subscribe(changes => {
                this.next({ ...changes, status: changes.status || 'Ready' });
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
        this.runTask(TASKS[0]);
    }

    public runTask(task: TaskDefinition | undefined) {
        if (!task || this.snapshot.status !== 'Started') {
            return;
        }

        this.next({ taskId: task.id });

        this.tourService.initialize(task.steps, DEFAULTS);
        this.tourService.start();
    }

    private isDoneOrNotReady() {
        return !this.snapshot.status || this.snapshot.status === 'Completed';
    }
}

function createTasks(completed?: Record<string, boolean>): TaskSnapshot[] {
    let wasCompleted = false;

    return TASKS.map(task => {
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

const TASKS: TaskDefinition[] = [{
    id: 'createApp',
    title: 'tour.createApp.title',
    description: 'tour.createApp.description',
    steps: [{
        anchorId: 'addApp',
        content: 'Use this button to create a new app.',
        title: 'Add a new app',
        nextOnAnchorClick: true,
        scrollContainer: '.panel-container',
    }, {
        anchorId: 'appForm',
        content: 'Other content',
        title: 'Second',
        nextOnCondition: waitForAnchor('app'),
        scrollContainer: '.panel-container',
    }, {
        anchorId: 'app',
        content: 'Click on your app',
        title: 'Second',
        nextOnAnchorClick: true,
        scrollContainer: '.panel-container',
    }, {
        anchorId: 'appDashboard',
        content: 'Other content',
        title: 'Second',
        isAsync: true,
        enableBackdrop: false,
    }],
}, {
    id: 'createSchema',
    title: 'tour.createSchema.title',
    description: 'tour.createSchema.description',
    steps: [{
        anchorId: 'addSchema',
        content: 'Use this button to create a new app.',
        title: 'Add a new app',
        nextOnAnchorClick: true,
        scrollContainer: '.panel-container',
    }, {
        anchorId: 'appForm',
        content: 'Other content',
        title: 'Second',
        nextOnCondition: waitForAnchor('app'),
        scrollContainer: '.panel-container',
    }, {
        anchorId: 'app',
        content: 'Other content',
        title: 'Second',
        duplicateAnchorHandling: 'registerFirst',
        scrollContainer: '.panel-container',
    }],
}, {
    id: 'createContent',
    title: 'tour.createContent.title',
    description: 'tour.createContent.description',
    steps: [{
        anchorId: 'selectSchema',
        content: 'Use this button to create a new app.',
        title: 'Add a new app',
        nextOnAnchorClick: true,
        scrollContainer: '.panel-container',
    }, {
        anchorId: 'appForm',
        content: 'Other content',
        title: 'Second',
    }],
}];

const DEFAULTS: StepDefinition = {
    allowUserInitiatedNavigation: true,
    enableBackdrop: true,
    duplicateAnchorHandling: 'registerLast',
};