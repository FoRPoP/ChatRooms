import React, { useState } from 'react';
import { Dialog, DialogType, DialogFooter, PrimaryButton, DefaultButton, TextField } from '@fluentui/react';
import { ChatApi } from '../api/apis/chat-api';

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

const CreateChatRoomModal: React.FC<ICreateChatRoomModalProps> = ({ isOpen, onClose, onCreate }) => {
    const [newRoomName, setNewRoomName] = useState('');
    const [newRoomDescription, setNewRoomDescription] = useState('');

    const chatApi = new ChatApi();

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