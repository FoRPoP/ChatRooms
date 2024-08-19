import React, { useState } from 'react';
import { Dialog, DialogType, DialogFooter, PrimaryButton, DefaultButton, TextField, Dropdown, IDropdownOption } from '@fluentui/react';
import { ChatApi } from '../api/apis/chat-api';
import axiosInstance from '../axiosConfig';
import { RegionsEnum } from '../api';

interface ICreateChatRoomModalProps {
    isOpen: boolean;
    onClose: () => void;
    onCreate: (roomName: string, roomDescription: string) => void;
}

const dialogContentProps = {
    type: DialogType.normal,
    title: 'Create a new Chat Room',
    closeButtonAriaLabel: 'Close',
};

const modalPropsStyles = { main: { maxWidth: 450 } };
    const dialogModalProps = {
    isBlocking: true,
    styles: modalPropsStyles,
};

const regionOptions: IDropdownOption[] = Object.values(RegionsEnum).map(region => ({
    key: region,
    text: region,
}));

const CreateChatRoomModal: React.FC<ICreateChatRoomModalProps> = ({ isOpen, onClose, onCreate }) => {
    const [newRoomName, setNewRoomName] = useState('');
    const [newRoomDescription, setNewRoomDescription] = useState('');

    return (
        <Dialog
            hidden={!isOpen}
            onDismiss={onClose}
            dialogContentProps={dialogContentProps}
            modalProps={dialogModalProps}
        >
            <TextField
                label="Room Name"
                value={newRoomName}
                onChange={(e, value) => setNewRoomName(value!)}
                required
            />
            <TextField
                label="Room Description"
                value={newRoomDescription}
                onChange={(e, value) => setNewRoomDescription(value!)}
                multiline
                rows={3}
                required
            />
            <DialogFooter>
                <PrimaryButton onClick={() => onCreate(newRoomName, newRoomDescription)} text="Create" />
                <DefaultButton onClick={onClose} text="Cancel" />
            </DialogFooter>
        </Dialog>
    )

};

export default CreateChatRoomModal;