import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';

@Component({
    selector: 'sqx-elasticsearch-action',
    styleUrls: ['./elasticsearch-action.component.scss'],
    templateUrl: './elasticsearch-action.component.html'
})
export class ElasticSearchActionComponent implements OnInit {
    @Input()
    public action: any;

    @Output()
    public actionChanged = new EventEmitter<object>();

    public actionFormSubmitted = false;
    public actionForm =
        this.formBuilder.group({
            indexName: ['',
                [
                    Validators.required
                ]],
            typeNameForSchema: ['',
                [
                    Validators.required
                ]]
        });

    constructor(
        private readonly formBuilder: FormBuilder
    ) {
    }

    public ngOnInit() {
        this.action = Object.assign({}, { url: '', sharedSecret: '' }, this.action || {});

        this.actionFormSubmitted = false;
        this.actionForm.reset();
        this.actionForm.setValue(this.action);
    }

    public save() {
        this.actionFormSubmitted = true;

        if (this.actionForm.valid) {
            const action = this.actionForm.value;

            this.actionChanged.emit(action);
        }
    }
}