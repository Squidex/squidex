import {Component, EventEmitter, Input, OnInit, Output} from '@angular/core';
import {FormBuilder, Validators} from '@angular/forms';

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
            hostUrl: ['', [
                Validators.required
            ]],
            requiresAuthentication: [''],
            username: [''],
            password: [''],
            indexName: ['',
                [
                    Validators.required
                ]],
            typeNameForSchema: ['',
                [
                    Validators.required
                ]]
        });

    constructor(private readonly formBuilder: FormBuilder) {
    }

    public ngOnInit() {
        this.action = Object.assign({}, {
            hostUrl: 'http://localhost:9200',
            requiresAuthentication: false,
            username: '',
            password: '',
            indexName: '$SCHEMA_NAME',
            typeNameForSchema: '$SCHEMA_NAME'
        }, this.action || {});

        this.actionFormSubmitted = false;
        this.actionForm.reset();
        this.actionForm.setValue(this.action);

        this.actionForm.controls['username'].disable();
        this.actionForm.controls['password'].disable();
    }

    public save() {
        this.actionFormSubmitted = true;

        if (this.actionForm.valid) {
            const action = this.actionForm.value;

            this.actionChanged.emit(action);
        }
    }

    public requiresAuthenticationChanged(e: any) {
        if (e.target.checked) {
            this.actionForm.controls['username'].enable();
            this.actionForm.controls['password'].enable();
        } else {
            this.actionForm.controls['username'].disable();
            this.actionForm.controls['password'].disable();
        }
    }
}